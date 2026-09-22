using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that stores playlists in the database and imports and exports them as M3U files.
/// </summary>
/// <param name="serviceScopeFactory">The scope factory used to resolve the scoped repository service per call.</param>
/// <param name="providerService">The provider service used for file access.</param>
/// <param name="serializerService">The serializer used to read and write M3U content.</param>
/// <param name="logoService">The service that knows which logos are cached.</param>
/// <param name="settings">The application settings, which say how a cached logo is exported.</param>
internal sealed class PlaylistService(
	IServiceScopeFactory serviceScopeFactory,
	IProviderService providerService,
	ISerializerService serializerService,
	ILogoService logoService,
	ApplicationSettings settings) : IPlaylistService
{
	public async Task<IPagedList<PlaylistSummaryResponse>> GetPlaylistsAsync(PlaylistSearchRequest? request = null, CancellationToken cancellationToken = default)
	{
		request ??= new PlaylistSearchRequest();

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		int totalCount = await repositoryService.Playlists
			.CountAsync(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		IReadOnlyList<PlaylistSummaryResponse> summaries = await repositoryService.Playlists
			.GetListAsync(
				p => new PlaylistSummaryResponse { Id = p.Id, Name = p.Name, EntryCount = p.Entries.Count },
				new Query<PlaylistEntity> { OrderBy = q => q.OrderBy(p => p.Name), Skip = request.Skip, Take = request.PageSize },
				cancellationToken)
			.ConfigureAwait(false);

		return new PagedList<PlaylistSummaryResponse>(summaries, totalCount, request.PageNumber, request.PageSize);
	}

	public async Task<IPlaylist?> LoadAsync(int id, CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		PlaylistEntity? entity = await repositoryService.Playlists
			.GetByIdAsync(id, new Query<PlaylistEntity> { Include = [p => p.Entries] }, cancellationToken)
			.ConfigureAwait(false);

		return entity?.ToModel();
	}

	public async Task<int> CreateAsync(string name, IPlaylist playlist, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		PlaylistEntity entity = playlist.ToEntity(name.Trim());

		await repositoryService.Playlists
			.CreateAsync(entity, cancellationToken)
			.ConfigureAwait(false);

		await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return entity.Id;
	}

	public async Task<bool> UpdateAsync(int id, string name, IPlaylist playlist, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		PlaylistEntity? entity = await repositoryService.Playlists
			.GetByIdAsync(id, new Query<PlaylistEntity> { Include = [p => p.Entries], TrackChanges = true }, cancellationToken)
			.ConfigureAwait(false);

		if (entity is null)
			return false;

		// Header, removed and added entries go into one commit, so a failure leaves the stored playlist untouched.
		entity.Name = name.Trim();
		entity.Apply(playlist);
		repositoryService.PlaylistEntries.Delete(entity.Entries);

		await repositoryService.PlaylistEntries
			.CreateAsync(playlist.ToEntryEntities(id), cancellationToken)
			.ConfigureAwait(false);

		await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return true;
	}

	public async Task<bool> RenameAsync(int id, string name, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		int updated = await repositoryService.Playlists
			.ExecuteUpdateAsync(id, setters => setters.SetProperty(p => p.Name, name.Trim()), cancellationToken)
			.ConfigureAwait(false);

		return updated > 0;
	}

	public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		// The entries are removed by the cascading foreign key.
		int deleted = await repositoryService.Playlists
			.ExecuteDeleteAsync(id, cancellationToken)
			.ConfigureAwait(false);

		return deleted > 0;
	}

	public async Task<int> ImportAsync(string filePath, string? name = null, CancellationToken cancellationToken = default)
	{
		string[] fileLines = await providerService.File
			.ReadAllLinesAsync(filePath, cancellationToken)
			.ConfigureAwait(false);

		IPlaylist playlist = serializerService.Deserialize(fileLines);

		return await CreateAsync(name ?? Path.GetFileNameWithoutExtension(filePath), playlist, cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<bool> ExportAsync(int id, string filePath, CancellationToken cancellationToken = default)
	{
		IPlaylist? playlist = await LoadAsync(id, cancellationToken)
			.ConfigureAwait(false);

		if (playlist is null)
			return false;

		await ApplyCachedLogosAsync(playlist, filePath, cancellationToken)
			.ConfigureAwait(false);

		string fileContent = serializerService.Serialize(playlist);

		await providerService.File
			.WriteAllTextAsync(filePath, fileContent, cancellationToken)
			.ConfigureAwait(false);

		return true;
	}

	/// <summary>
	/// Points the <c>tvg-logo</c> of every entry whose logo is cached at the cached file.
	/// </summary>
	/// <remarks>
	/// The playlist is the copy that was loaded for the export, the stored one keeps its URLs.
	/// </remarks>
	/// <param name="playlist">The playlist that is about to be written.</param>
	/// <param name="filePath">The file the playlist is written to, a relative path starts there.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	private async Task ApplyCachedLogosAsync(IPlaylist playlist, string filePath, CancellationToken cancellationToken)
	{
		if (!settings.Logo.UseLocalPathsOnExport)
			return;

		IReadOnlyDictionary<string, string> pathsByUrl = await logoService
			.GetPathsByUrlAsync(cancellationToken)
			.ConfigureAwait(false);

		if (pathsByUrl.Count is 0)
			return;

		string? directory = Path.GetDirectoryName(Path.GetFullPath(filePath));

		foreach (EntryModel entry in playlist.Entries)
		{
			if (entry.Metadata.TvgLogo is not { } logo || !pathsByUrl.TryGetValue(logo, out string? localPath))
				continue;

			entry.Metadata.TvgLogo = settings.Logo.ExportPathStyle is LogoPathStyle.Relative && directory is not null
				? Path.GetRelativePath(directory, localPath)
				: localPath;
		}
	}

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}