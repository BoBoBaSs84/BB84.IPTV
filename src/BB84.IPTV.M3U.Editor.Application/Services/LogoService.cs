// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;
using System.Security.Cryptography;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that keeps the channel logos on disk.
/// </summary>
/// <remarks>
/// One logo is cached per channel, the one <see cref="LogoSelector"/> picks, so a playlist shows
/// and exports local files instead of asking the network.
/// </remarks>
/// <param name="serviceScopeFactory">The scope factory used to resolve the scoped repository service per call.</param>
/// <param name="downloadService">The service that downloads a logo.</param>
/// <param name="logoStoreService">The store that holds the downloaded files.</param>
/// <param name="providerService">The provider service used for file access.</param>
/// <param name="eventService">The service that publishes the progress.</param>
internal sealed class LogoService(
	IServiceScopeFactory serviceScopeFactory,
	IDownloadService downloadService,
	ILogoStoreService logoStoreService,
	IProviderService providerService,
	IEventService eventService) : ILogoService
{
	public async Task<LogoCacheStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> logos = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity>(), cancellationToken)
			.ConfigureAwait(false);

		List<LogoEntity> selected = [.. SelectPerChannel(logos)];
		List<LogoEntity> cached = [.. selected.Where(logo => logoStoreService.Exists(logo.LocalPath))];

		return new LogoCacheStatusResponse
		{
			TotalCount = selected.Count,
			CachedCount = cached.Count,
			CachedBytes = cached.Sum(logo => logo.FileSize ?? 0)
		};
	}

	public async Task<int> CacheLogosAsync(LogoCacheRequest? request = null, CancellationToken cancellationToken = default)
	{
		request ??= new LogoCacheRequest();

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> logos = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity> { TrackChanges = true }, cancellationToken)
			.ConfigureAwait(false);

		List<LogoEntity> candidates = [.. SelectPerChannel(logos)
			.Where(logo => request.Channels.Count is 0 || request.Channels.Contains(logo.Channel, StringComparer.OrdinalIgnoreCase))
			.Where(logo => request.RefreshCached || !logoStoreService.Exists(logo.LocalPath))];

		int downloaded = 0;
		int done = 0;

		// The logos of a batch are downloaded at once, the database is written afterwards: the
		// context belongs to one thread, and a committed batch is what a cancelled run keeps.
		foreach (LogoEntity[] batch in candidates.Chunk(request.MaxParallelDownloads))
		{
			// A cancelled run keeps what it downloaded so far, so the next one goes on from there.
			if (cancellationToken.IsCancellationRequested)
				break;

			LogoUpdate?[] updates = await Task
				.WhenAll(batch.Select(logo => DownloadAsync(logo, cancellationToken)))
				.ConfigureAwait(false);

			for (int index = 0; index < batch.Length; index++)
			{
				if (Apply(batch[index], updates[index]))
					downloaded++;
			}

			done += batch.Length;
			PublishProgress(done, candidates.Count);

			_ = await repositoryService
				.CommitChangesAsync(CancellationToken.None)
				.ConfigureAwait(false);
		}

		if (downloaded > 0)
			eventService.Publish(new LogoCacheChangedEvent(downloaded));

		return downloaded;
	}

	public async Task<string?> GetLocalPathAsync(string channel, string? feed = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channel);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> logos = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity> { Where = logo => logo.Channel == channel }, cancellationToken)
			.ConfigureAwait(false);

		LogoEntity? logo = LogoSelector.Select(logos.Where(logo => logoStoreService.Exists(logo.LocalPath)), feed);

		return logo?.LocalPath;
	}

	public async Task<IReadOnlyDictionary<string, string>> GetPathsByUrlAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> logos = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity> { Where = logo => logo.LocalPath != null }, cancellationToken)
			.ConfigureAwait(false);

		Dictionary<string, string> pathsByUrl = new(StringComparer.OrdinalIgnoreCase);

		foreach (LogoEntity logo in logos.Where(logo => logoStoreService.Exists(logo.LocalPath)))
			pathsByUrl[logo.Url] = logo.LocalPath!;

		return pathsByUrl;
	}

	public async Task<int> ClearAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		int deleted = logoStoreService.Clear();

		_ = await repositoryService.Logos
			.ExecuteUpdateAsync(
				logo => logo.LocalPath != null,
				setters => setters
					.SetProperty(logo => logo.LocalPath, (string?)null)
					.SetProperty(logo => logo.ETag, (string?)null)
					.SetProperty(logo => logo.ContentHash, (string?)null)
					.SetProperty(logo => logo.FileSize, (long?)null)
					.SetProperty(logo => logo.DownloadedAt, (DateTime?)null),
				cancellationToken)
			.ConfigureAwait(false);

		eventService.Publish(new LogoCacheChangedEvent(0));

		return deleted;
	}

	/// <summary>
	/// Downloads one logo and writes it to the store, without touching the row.
	/// </summary>
	/// <remarks>
	/// Runs next to the downloads of the same batch, so it only uses what is safe to share: the
	/// downloader, the store and the row values it reads.
	/// </remarks>
	/// <returns>What the row is to be set to, or <see langword="null"/> if nothing changed.</returns>
	private async Task<LogoUpdate?> DownloadAsync(LogoEntity logo, CancellationToken cancellationToken)
	{
		LogoDownloadResponse? download = await downloadService
			.DownloadAsync(logo.Url, logoStoreService.Exists(logo.LocalPath) ? logo.ETag : null, cancellationToken)
			.ConfigureAwait(false);

		// Unreachable or unchanged: the cached file stays as it is.
		if (download is null || download.NotModified || download.Content.Length is 0)
			return null;

		string hash = Convert.ToHexStringLower(SHA256.HashData(download.Content));

		// The same file came back, only the entity tag is worth keeping.
		if (string.Equals(hash, logo.ContentHash, StringComparison.OrdinalIgnoreCase) && logoStoreService.Exists(logo.LocalPath))
			return new LogoUpdate(null, download.ETag ?? logo.ETag, logo.ContentHash, logo.FileSize, logo.DownloadedAt);

		string path = await logoStoreService
			.SaveAsync(logo.Channel, BuildFileName(logo, download.ContentType), download.Content, cancellationToken)
			.ConfigureAwait(false);

		return new LogoUpdate(path, download.ETag, hash, download.Content.Length, providerService.DateTime.UtcNow);
	}

	/// <summary>
	/// Writes what a download brought to the row, on the thread the context belongs to.
	/// </summary>
	/// <returns><see langword="true"/> if a file was written.</returns>
	private static bool Apply(LogoEntity logo, LogoUpdate? update)
	{
		if (update is null)
			return false;

		if (update.LocalPath is null)
		{
			logo.ETag = update.ETag;
			return false;
		}

		logo.LocalPath = update.LocalPath;
		logo.ETag = update.ETag;
		logo.ContentHash = update.ContentHash;
		logo.FileSize = update.FileSize;
		logo.DownloadedAt = update.DownloadedAt;

		return true;
	}

	/// <summary>
	/// What a download brought, before it is written to the row.
	/// </summary>
	/// <param name="LocalPath">The written file, <see langword="null"/> if the cached one still fits.</param>
	/// <param name="ETag">The entity tag the server sent.</param>
	/// <param name="ContentHash">The hash of the file.</param>
	/// <param name="FileSize">The size of the file in bytes.</param>
	/// <param name="DownloadedAt">The moment the file was downloaded.</param>
	private sealed record LogoUpdate(string? LocalPath, string? ETag, string? ContentHash, long? FileSize, DateTime? DownloadedAt);

	/// <summary>
	/// Builds the file name of a logo: the feed or the channel, with the extension of the URL, of
	/// the media type the server reported or of the format the catalog knows.
	/// </summary>
	private static string BuildFileName(LogoEntity logo, string? contentType)
	{
		string name = string.IsNullOrWhiteSpace(logo.Feed) ? logo.Channel : $"{logo.Channel}@{logo.Feed}";
		string extension = GetExtension(logo, contentType);

		return $"{name}{extension}";
	}

	private static string GetExtension(LogoEntity logo, string? contentType)
	{
		string fromUrl = Path.GetExtension(new Uri(logo.Url, UriKind.RelativeOrAbsolute).IsAbsoluteUri
			? new Uri(logo.Url).AbsolutePath
			: logo.Url);

		if (!string.IsNullOrWhiteSpace(fromUrl) && fromUrl.Length <= 5)
			return fromUrl.ToLowerInvariant();

		string? fromContentType = contentType?.Split(';')[0].Trim().ToLowerInvariant() switch
		{
			"image/png" => ".png",
			"image/jpeg" => ".jpg",
			"image/webp" => ".webp",
			"image/gif" => ".gif",
			"image/avif" => ".avif",
			"image/svg+xml" => ".svg",
			_ => null
		};

		return fromContentType ?? (string.IsNullOrWhiteSpace(logo.Format) ? ".img" : $".{logo.Format.ToLowerInvariant()}");
	}

	/// <summary>
	/// Reduces the logos to the one that is used per channel.
	/// </summary>
	private static IEnumerable<LogoEntity> SelectPerChannel(IEnumerable<LogoEntity> logos)
		=> logos
			.GroupBy(logo => logo.Channel, StringComparer.OrdinalIgnoreCase)
			.Select(group => LogoSelector.Select(group))
			.OfType<LogoEntity>();

	private void PublishProgress(int done, int total)
	{
		int percentage = total is 0 ? 100 : (int)Math.Round(done / (double)total * 100, MidpointRounding.AwayFromZero);

		eventService.Publish(new LogoCacheProgressEvent(done, total, percentage));
		eventService.Publish(new ProgressChangedEvent(percentage));
	}

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}