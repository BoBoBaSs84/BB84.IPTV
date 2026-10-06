// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
/// <param name="logger">The logger that writes what went wrong with a single logo.</param>
internal sealed class LogoService(
	IServiceScopeFactory serviceScopeFactory,
	IDownloadService downloadService,
	ILogoStoreService logoStoreService,
	IProviderService providerService,
	IEventService eventService,
	ILogger<LogoService> logger) : ILogoService
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

		List<LogoCandidate> candidates = await SelectCandidatesAsync(request, cancellationToken)
			.ConfigureAwait(false);

		int downloaded = 0;
		int failed = 0;
		int done = 0;

		// The logos of a batch are downloaded at once, the database is written afterwards: the
		// context belongs to one thread, and a committed batch is what a cancelled run keeps.
		foreach (LogoCandidate[] batch in candidates.Chunk(request.MaxParallelDownloads))
		{
			// A cancelled run keeps what it downloaded so far, so the next one goes on from there.
			if (cancellationToken.IsCancellationRequested)
				break;

			LogoResult[] results;

			try
			{
				results = await Task
					.WhenAll(batch.Select(candidate => DownloadAsync(candidate, cancellationToken)))
					.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Stopping a run is not a failure: what the batches before committed is kept, and
				// the next run goes on from there.
				break;
			}

			failed += results.Count(result => result.Failed);
			downloaded += await WriteBatchAsync(batch, results).ConfigureAwait(false);

			done += batch.Length;
			PublishProgress(done, candidates.Count);
		}

		if (downloaded > 0)
			eventService.Publish(new LogoCacheChangedEvent(downloaded));

		// Every logo that was skipped is in the log; the run reports them once at the end, because
		// one report per logo would bury the user in dialogs.
		if (failed > 0)
			eventService.Publish(new WarningOccuredEvent(Resources.LogoCacheSkipped.FormatMessage(failed)));

		return downloaded;
	}

	public async Task<IPagedList<LogoOptionResponse>> SearchLogosAsync(LogoSearchRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		Expression<Func<LogoEntity, bool>> filter = await BuildFilterAsync(repositoryService, request, cancellationToken)
			.ConfigureAwait(false);

		int total = await repositoryService.Logos
			.CountAsync(new Query<LogoEntity> { Where = filter }, cancellationToken)
			.ConfigureAwait(false);

		// Only the page is read, the catalog knows several logos for most of its channels.
		IReadOnlyList<LogoEntity> page = await repositoryService.Logos
			.GetListAsync(
				new Query<LogoEntity>
				{
					Where = filter,
					OrderBy = query => query.OrderBy(logo => logo.Channel).ThenBy(logo => logo.Id),
					Skip = request.Skip,
					Take = request.PageSize
				},
				cancellationToken)
			.ConfigureAwait(false);

		// What the catalog knows about the channels of the page, so a row can be recognised.
		List<string> channels = [.. page
			.Select(logo => logo.Channel)
			.Distinct(StringComparer.OrdinalIgnoreCase)];

		Dictionary<string, ChannelInfo> channelsById = await CatalogLookup.LoadChannelsAsync(repositoryService, channels, cancellationToken)
			.ConfigureAwait(false);

		// The row says that the logo was downloaded, the store says whether the file is still there.
		IEnumerable<LogoOptionResponse> options = page
			.Select(logo => logo.ToOption(CatalogLookup.Lookup(channelsById, logo.Channel), logoStoreService.Exists(logo.LocalPath)));

		return new PagedList<LogoOptionResponse>(options, total, request.PageNumber, request.PageSize);
	}

	public async Task<string?> CacheLogoAsync(int logoId, CancellationToken cancellationToken = default)
	{
		LogoCandidate? candidate = await LoadCandidateAsync(logoId, cancellationToken)
			.ConfigureAwait(false);

		if (candidate is null)
			return null;

		LogoResult result = await DownloadAsync(candidate, cancellationToken)
			.ConfigureAwait(false);

		if (await WriteBatchAsync([candidate], [result]).ConfigureAwait(false) > 0)
			eventService.Publish(new LogoCacheChangedEvent(1));

		if (result.Failed)
			eventService.Publish(new WarningOccuredEvent(Resources.LogoCacheSkipped.FormatMessage(1)));

		// The download brought a file, or the server reported that the cached one still fits.
		return result.Update?.LocalPath
			?? (logoStoreService.Exists(candidate.LocalPath) ? candidate.LocalPath : null);
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
	/// Builds what a search covers: the channel, what the cache holds and the text a logo must hold.
	/// </summary>
	/// <remarks>
	/// The text is matched on the columns of the logo and on the name of its channel, which lives in
	/// a table of its own: the channels whose name holds the text are read first, so the filter the
	/// database runs can name them. A text of the form <c>channel@feed</c> is the <c>tvg-id</c> of an
	/// entry, and a feed only decides which logo of a channel is preferred, so the channel alone is
	/// matched and the logos of its other feeds stay in the result.
	/// </remarks>
	private static async Task<Expression<Func<LogoEntity, bool>>> BuildFilterAsync(
		IRepositoryService repositoryService,
		LogoSearchRequest request,
		CancellationToken cancellationToken)
	{
		string? text = request.SearchText.TrimToNull();
		string? channel = request.Channel.TrimToNull() ?? (text.TryParseChannelFeed(out string tvgChannel, out _) ? tvgChannel : null);

		// The text named the channel, so it is not matched as a text as well.
		if (channel is not null)
			text = null;

		List<string> named = text is null
			? []
			: [.. await LoadChannelsByNameAsync(repositoryService, text, cancellationToken).ConfigureAwait(false)];

		LogoCacheFilter cacheState = request.CacheState;

		return logo
			=> (channel == null || logo.Channel == channel)
			&& (cacheState != LogoCacheFilter.Cached || logo.LocalPath != null)
			&& (cacheState != LogoCacheFilter.NotCached || logo.LocalPath == null)
			&& (text == null
				|| logo.Channel.Contains(text)
				|| named.Contains(logo.Channel)
				|| (logo.Feed != null && logo.Feed.Contains(text))
				|| (logo.Format != null && logo.Format.Contains(text))
				|| logo.Url.Contains(text)
				|| (logo.LocalPath != null && logo.LocalPath.Contains(text)));
	}

	/// <summary>
	/// Reads the identifiers of the channels whose name holds the text.
	/// </summary>
	private static async Task<IReadOnlyList<string>> LoadChannelsByNameAsync(
		IRepositoryService repositoryService,
		string text,
		CancellationToken cancellationToken)
		=> await repositoryService.Channels
			.GetListAsync(
				Mappings.ChannelToIdentifier,
				new Query<ChannelEntity> { Where = channel => channel.Name.Contains(text) },
				cancellationToken)
			.ConfigureAwait(false);

	/// <summary>
	/// Reads which logos are to be downloaded.
	/// </summary>
	/// <remarks>
	/// The whole table is read to pick one logo per channel, but only for the moment of the
	/// selection and untracked: a run that downloads thousands of files goes on with the few
	/// values a download needs, instead of holding every row and its change tracking until it is
	/// through.
	/// </remarks>
	/// <param name="request">What the run is to cache.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The logos to download, one per channel.</returns>
	private async Task<List<LogoCandidate>> SelectCandidatesAsync(LogoCacheRequest request, CancellationToken cancellationToken)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> logos = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity>(), cancellationToken)
			.ConfigureAwait(false);

		return
		[
			.. SelectPerChannel(logos)
				.Where(logo => request.Channels.Count is 0 || request.Channels.Contains(logo.Channel, StringComparer.OrdinalIgnoreCase))
				.Where(logo => request.RefreshCached || !logoStoreService.Exists(logo.LocalPath))
				.Select(ToCandidate)
		];
	}

	/// <summary>
	/// Reads the one logo a single download works on.
	/// </summary>
	/// <param name="logoId">The identifier of the stored logo.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The values the download works with, or <see langword="null"/> if the logo is gone.</returns>
	private async Task<LogoCandidate?> LoadCandidateAsync(int logoId, CancellationToken cancellationToken)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		LogoEntity? logo = await repositoryService.Logos
			.GetByIdAsync(logoId, new Query<LogoEntity>(), cancellationToken)
			.ConfigureAwait(false);

		return logo is null ? null : ToCandidate(logo);
	}

	/// <summary>
	/// Writes what the downloads of one batch brought to the rows they belong to.
	/// </summary>
	/// <remarks>
	/// The rows are loaded tracked in a scope of their own, so the change tracker holds one batch
	/// and lets it go again with the scope. A row that is gone, e.g. because the catalog was
	/// synchronized while the run was going on, is left alone.
	/// </remarks>
	/// <param name="batch">The logos that were downloaded.</param>
	/// <param name="results">What each download of the batch brought.</param>
	/// <returns>The number of logos that were written to the store.</returns>
	private async Task<int> WriteBatchAsync(LogoCandidate[] batch, LogoResult[] results)
	{
		int[] ids = [.. batch
			.Where((_, index) => results[index].Update is not null)
			.Select(candidate => candidate.Id)];

		// Nothing came back that is worth a row, e.g. every logo of the batch was unreachable.
		if (ids.Length is 0)
			return 0;

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<LogoEntity> tracked = await repositoryService.Logos
			.GetListAsync(new Query<LogoEntity> { Where = logo => ids.Contains(logo.Id), TrackChanges = true }, CancellationToken.None)
			.ConfigureAwait(false);

		Dictionary<int, LogoEntity> entitiesById = tracked.ToDictionary(logo => logo.Id);

		int downloaded = 0;
		for (int index = 0; index < batch.Length; index++)
		{
			if (entitiesById.TryGetValue(batch[index].Id, out LogoEntity? entity) && Apply(entity, results[index].Update))
				downloaded++;
		}

		_ = await repositoryService
			.CommitChangesAsync(CancellationToken.None)
			.ConfigureAwait(false);

		return downloaded;
	}

	/// <summary>
	/// Downloads one logo and writes it to the store, without touching the row.
	/// </summary>
	/// <remarks>
	/// Runs next to the downloads of the same batch, so it only uses what is safe to share: the
	/// downloader, the store and the row values it reads.
	/// </remarks>
	/// <returns>What the row is to be set to, and whether the logo had to be skipped.</returns>
	private async Task<LogoResult> DownloadAsync(LogoCandidate logo, CancellationToken cancellationToken)
	{
		try
		{
			LogoDownloadResponse? download = await downloadService
				.DownloadAsync(logo.Url, logoStoreService.Exists(logo.LocalPath) ? logo.ETag : null, cancellationToken)
				.ConfigureAwait(false);

			// Unreachable or unchanged: the cached file stays as it is.
			if (download is null || download.NotModified || download.Content.Length is 0)
				return LogoResult.Nothing;

			string hash = Convert.ToHexStringLower(SHA256.HashData(download.Content));

			// The same file came back, only the entity tag is worth keeping.
			if (string.Equals(hash, logo.ContentHash, StringComparison.OrdinalIgnoreCase) && logoStoreService.Exists(logo.LocalPath))
				return new LogoResult(new LogoUpdate(null, download.ETag ?? logo.ETag, logo.ContentHash, logo.FileSize, logo.DownloadedAt), false);

			string path = await logoStoreService
				.SaveAsync(logo.Channel, BuildFileName(logo, download.ContentType), download.Content, cancellationToken)
				.ConfigureAwait(false);

			return new LogoResult(new LogoUpdate(path, download.ETag, hash, download.Content.Length, providerService.DateTime.UtcNow), false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			// Writing the file failed, e.g. the disk is full or the path is refused. One logo is
			// not worth ending the run for: the row keeps what it had and the next run tries again.
			Log.LogoCacheFailed(logger, logo.Url, exception);
			return LogoResult.Failure;
		}
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
	/// Reduces a stored logo to what a download needs, so a run does not hold the rows.
	/// </summary>
	/// <param name="logo">The logo that was picked for a channel.</param>
	/// <returns>The values the download works with.</returns>
	private static LogoCandidate ToCandidate(LogoEntity logo)
		=> new(logo.Id, logo.Channel, logo.Feed, logo.Format, logo.Url, logo.LocalPath, logo.ETag, logo.ContentHash, logo.FileSize, logo.DownloadedAt);

	/// <summary>
	/// The logo of one channel, as a run works with it.
	/// </summary>
	/// <param name="Id">The identifier of the row the download belongs to.</param>
	/// <param name="Channel">The channel the logo belongs to.</param>
	/// <param name="Feed">The feed the logo belongs to, if any.</param>
	/// <param name="Format">The image format the catalog knows.</param>
	/// <param name="Url">The URL the logo is downloaded from.</param>
	/// <param name="LocalPath">The path of the file that is already cached, if any.</param>
	/// <param name="ETag">The entity tag of the cached file, if any.</param>
	/// <param name="ContentHash">The hash of the cached file, if any.</param>
	/// <param name="FileSize">The size of the cached file in bytes, if any.</param>
	/// <param name="DownloadedAt">The moment the cached file was downloaded, if any.</param>
	private sealed record LogoCandidate(
		int Id,
		string Channel,
		string? Feed,
		string? Format,
		string Url,
		string? LocalPath,
		string? ETag,
		string? ContentHash,
		long? FileSize,
		DateTime? DownloadedAt);

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
	/// What one logo of a batch came to.
	/// </summary>
	/// <param name="Update">What the row is to be set to, <see langword="null"/> if nothing changed.</param>
	/// <param name="Failed">Whether the logo had to be skipped because something went wrong.</param>
	private sealed record LogoResult(LogoUpdate? Update, bool Failed)
	{
		/// <summary>
		/// The logo is already cached, unreachable or unchanged, so the row stays as it is.
		/// </summary>
		public static LogoResult Nothing { get; } = new(null, false);

		/// <summary>
		/// The logo could not be cached, which is logged and reported at the end of the run.
		/// </summary>
		public static LogoResult Failure { get; } = new(null, true);
	}

	/// <summary>
	/// Builds the file name of a logo: the feed or the channel and the identifier of the row, with
	/// the extension of the URL, of the media type the server reported or of the format the catalog
	/// knows.
	/// </summary>
	/// <remarks>
	/// The identifier is part of the name, because a channel can hold several logos that only differ
	/// in their tags, and a single download of such a logo must not write over the file of the one
	/// that is picked for the channel. A file that was cached under the former name keeps its row,
	/// so nothing is downloaded again for it.
	/// </remarks>
	private string BuildFileName(LogoCandidate logo, string? contentType)
	{
		string name = string.IsNullOrWhiteSpace(logo.Feed) ? logo.Channel : $"{logo.Channel}@{logo.Feed}";
		string extension = GetExtension(logo, contentType);

		return $"{name}-{logo.Id}{extension}";
	}

	private string GetExtension(LogoCandidate logo, string? contentType)
	{
		string? fromUrl = providerService.Path.GetExtension(new Uri(logo.Url, UriKind.RelativeOrAbsolute).IsAbsoluteUri
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