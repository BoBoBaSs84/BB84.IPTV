// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the database service, which is responsible for managing the application's data storage and retrieval.
/// </summary>
/// <remarks>
/// The synchronization reads every list of the iptv-org catalog and brings the stored catalog in
/// line with it: a record that is new is added, a record that changed is updated, and a row that is
/// gone upstream is removed. A list that comes back without a single record is left alone, because
/// a failed request looks the same as an empty list.
/// </remarks>
internal sealed class DatabaseService : IDatabaseService, IDisposable
{
	/// <summary>
	/// The number of lists the catalog is read from.
	/// </summary>
	private const int TotalSyncTasks = 8;

	private readonly IEventService _eventService;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IProviderService _providerService;
	private readonly ILogoStoreService _logoStoreService;
	private readonly ApplicationSettings _applicationSettings;

	// A run writes the whole catalog, so the startup check and the button never run at the same time.
	private readonly SemaphoreSlim _gate = new(1, 1);

	/// <summary>
	/// Initializes a new instance of the <see cref="DatabaseService"/> class.
	/// </summary>
	/// <param name="eventService">The event service, for publishing what the database does.</param>
	/// <param name="serviceScopeFactory">The scope factory used to resolve scoped infrastructure services.</param>
	/// <param name="providerService">The provider service, which supplies the clock.</param>
	/// <param name="logoStoreService">The store that holds the downloaded logo files.</param>
	/// <param name="applicationSettings">The settings, which say how large a batch may be.</param>
	public DatabaseService(
		IEventService eventService,
		IServiceScopeFactory serviceScopeFactory,
		IProviderService providerService,
		ILogoStoreService logoStoreService,
		ApplicationSettings applicationSettings)
	{
		_eventService = eventService;
		_serviceScopeFactory = serviceScopeFactory;
		_providerService = providerService;
		_logoStoreService = logoStoreService;
		_applicationSettings = applicationSettings;
	}

	/// <summary>
	/// Releases what guards a run against a second one.
	/// </summary>
	public void Dispose()
		=> _gate.Dispose();

	public async Task<bool> CheckDatabaseAvailabilityAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);
		return await repositoryService.CanConnectAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			using IServiceScope scope = _serviceScopeFactory.CreateScope();
			IRepositoryService repositoryService = GetRepositoryService(scope);

			await repositoryService.MigrateDatabaseAsync(cancellationToken).ConfigureAwait(false);
			await repositoryService.ResetCatalogAsync(cancellationToken).ConfigureAwait(false);

			return true;
		}
		finally
		{
			_ = _gate.Release();
		}
	}

	public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);
		await repositoryService.MigrateDatabaseAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task<CatalogSyncResponse> SynchronizeAsync(CancellationToken cancellationToken = default)
	{
		await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

		try
		{
			List<CatalogKindResponse> kinds = [];

			kinds.Add(await SynchronizeKindAsync<CategoryRequest, CategoryEntity>(
				CatalogKind.Category, kinds.Count,
				(web, token) => web.GetCategoriesAsync(token),
				repository => repository.Categories,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<CountryRequest, CountryEntity>(
				CatalogKind.Country, kinds.Count,
				(web, token) => web.GetCountriesAsync(token),
				repository => repository.Countries,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<LanguageRequest, LanguageEntity>(
				CatalogKind.Language, kinds.Count,
				(web, token) => web.GetLanguagesAsync(token),
				repository => repository.Languages,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<ChannelRequest, ChannelEntity>(
				CatalogKind.Channel, kinds.Count,
				(web, token) => web.GetChannelsAsync(token),
				repository => repository.Channels,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<FeedRequest, FeedEntity>(
				CatalogKind.Feed, kinds.Count,
				(web, token) => web.GetFeedsAsync(token),
				repository => repository.Feeds,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<GuideRequest, GuideEntity>(
				CatalogKind.Guide, kinds.Count,
				(web, token) => web.GetGuidesAsync(token),
				repository => repository.Guides,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<LogoRequest, LogoEntity>(
				CatalogKind.Logo, kinds.Count,
				(web, token) => web.GetLogosAsync(token),
				repository => repository.Logos,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				DeleteCachedLogos, cancellationToken).ConfigureAwait(false));

			kinds.Add(await SynchronizeKindAsync<StreamRequest, StreamEntity>(
				CatalogKind.Stream, kinds.Count,
				(web, token) => web.GetStreamsAsync(token),
				repository => repository.Streams,
				request => request.GetKey(), entity => entity.GetKey(),
				request => request.ToEntity(), (entity, request) => entity.Apply(request),
				null, cancellationToken).ConfigureAwait(false));

			return new CatalogSyncResponse { Kinds = kinds };
		}
		finally
		{
			_ = _gate.Release();
		}
	}

	public async Task<IReadOnlyList<CatalogStatusResponse>> GetCatalogStatusAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<CatalogSyncEntity> stored = await repositoryService.CatalogSyncs
			.GetListAsync(new Query<CatalogSyncEntity>(), cancellationToken)
			.ConfigureAwait(false);

		Dictionary<CatalogKind, CatalogSyncEntity> storedByKind = stored
			.GroupBy(entity => entity.Kind)
			.ToDictionary(group => group.Key, group => group.First());

		// Every list is shown, also the ones that were never read.
		return [.. Enum.GetValues<CatalogKind>()
			.Select(kind => storedByKind.TryGetValue(kind, out CatalogSyncEntity? entity)
				? entity.ToResponse()
				: new CatalogStatusResponse { Kind = kind })];
	}

	/// <summary>
	/// Reads one list of the catalog and brings the stored rows in line with it.
	/// </summary>
	private async Task<CatalogKindResponse> SynchronizeKindAsync<TRequest, TEntity>(
		CatalogKind kind,
		int completedTasks,
		Func<IWebService, CancellationToken, Task<IEnumerable<TRequest>>> fetch,
		Func<IRepositoryService, IRepositoryBase<TEntity>> repository,
		Func<TRequest, string> requestKey,
		Func<TEntity, string> entityKey,
		Func<TRequest, TEntity> create,
		Func<TEntity, TRequest, bool> apply,
		Func<IReadOnlyList<TEntity>, IReadOnlyList<TEntity>, CancellationToken, Task>? onRemoved,
		CancellationToken cancellationToken)
		where TEntity : EntityBase
	{
		List<TRequest> imported = [.. await FetchAsync(fetch, cancellationToken).ConfigureAwait(false)];

		// The web service answers a failed request with an empty list, so nothing is removed for one.
		if (imported.Count is 0)
		{
			_eventService.Publish(new WarningOccuredEvent(Resources.DatabaseSyncEmptyResult.FormatMessage(kind.ToString())));
			CatalogKindResponse skipped = new() { Kind = kind, Outcome = CatalogSyncOutcome.Skipped };
			ReportProgress(skipped, completedTasks + 1);

			return skipped;
		}

		Dictionary<string, TRequest> importedByKey = new(StringComparer.Ordinal);
		int duplicates = 0;

		foreach (TRequest request in imported)
		{
			if (!importedByKey.TryAdd(requestKey(request), request))
				duplicates++;
		}

		IReadOnlyList<TEntity> stored = await ReadStoredAsync(repository, cancellationToken).ConfigureAwait(false);

		List<TEntity> created = [];
		List<TEntity> removed = [];
		List<TEntity> kept = [];
		Dictionary<int, TRequest> changed = [];
		int unchanged = 0;

		// The lowest identity of a key wins, so rows an earlier import stored twice are cleaned up.
		foreach (IGrouping<string, TEntity> group in stored.OrderBy(entity => entity.Id).GroupBy(entityKey, StringComparer.Ordinal))
		{
			TEntity entity = group.First();
			removed.AddRange(group.Skip(1));

			if (!importedByKey.Remove(group.Key, out TRequest? request))
			{
				removed.Add(entity);
				continue;
			}

			kept.Add(entity);

			if (apply(entity, request))
				changed.Add(entity.Id, request);
			else
				unchanged++;
		}

		created.AddRange(importedByKey.Values.Select(create));

		await InsertAsync(repository, created, cancellationToken).ConfigureAwait(false);
		await UpdateAsync(repository, changed, apply, cancellationToken).ConfigureAwait(false);
		await DeleteAsync(repository, removed, cancellationToken).ConfigureAwait(false);

		if (onRemoved is not null && removed.Count > 0)
			await onRemoved(removed, kept, cancellationToken).ConfigureAwait(false);

		CatalogKindResponse response = new()
		{
			Kind = kind,
			Outcome = CatalogSyncOutcome.Synchronized,
			Added = created.Count,
			Updated = changed.Count,
			Removed = removed.Count,
			Unchanged = unchanged,
			Duplicates = duplicates
		};

		await WriteStatusAsync(response, cancellationToken).ConfigureAwait(false);
		ReportProgress(response, completedTasks + 1);

		return response;
	}

	/// <summary>
	/// Reads the list from iptv-org.
	/// </summary>
	private async Task<IEnumerable<TRequest>> FetchAsync<TRequest>(
		Func<IWebService, CancellationToken, Task<IEnumerable<TRequest>>> fetch,
		CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<TRequest> result = await fetch(webService, cancellationToken).ConfigureAwait(false);

		// The web service swallows a cancellation as well, so it is checked here.
		cancellationToken.ThrowIfCancellationRequested();

		return result;
	}

	/// <summary>
	/// Reads the stored rows of one list, untracked, so the whole table can be held at once.
	/// </summary>
	private async Task<IReadOnlyList<TEntity>> ReadStoredAsync<TEntity>(
		Func<IRepositoryService, IRepositoryBase<TEntity>> repository,
		CancellationToken cancellationToken)
		where TEntity : EntityBase
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		return await repository(repositoryService)
			.GetListAsync(new Query<TEntity>(), cancellationToken)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Writes the new rows, one batch per scope, so the change tracker stays small.
	/// </summary>
	private async Task InsertAsync<TEntity>(
		Func<IRepositoryService, IRepositoryBase<TEntity>> repository,
		IReadOnlyList<TEntity> entities,
		CancellationToken cancellationToken)
		where TEntity : EntityBase
	{
		foreach (TEntity[] batch in entities.Chunk(GetBatchSize()))
		{
			cancellationToken.ThrowIfCancellationRequested();

			using IServiceScope scope = _serviceScopeFactory.CreateScope();
			IRepositoryService repositoryService = GetRepositoryService(scope);

			await repository(repositoryService).CreateAsync(batch, CancellationToken.None).ConfigureAwait(false);
			_ = await repositoryService.CommitChangesAsync(CancellationToken.None).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Writes the rows that changed, by loading them tracked again, so only the changed columns are
	/// written and the columns the application owns stay as they are.
	/// </summary>
	private async Task UpdateAsync<TEntity, TRequest>(
		Func<IRepositoryService, IRepositoryBase<TEntity>> repository,
		Dictionary<int, TRequest> changed,
		Func<TEntity, TRequest, bool> apply,
		CancellationToken cancellationToken)
		where TEntity : EntityBase
	{
		foreach (int[] batch in changed.Keys.Chunk(GetBatchSize()))
		{
			cancellationToken.ThrowIfCancellationRequested();

			using IServiceScope scope = _serviceScopeFactory.CreateScope();
			IRepositoryService repositoryService = GetRepositoryService(scope);

			IReadOnlyList<TEntity> tracked = await repository(repositoryService)
				.GetListAsync(new Query<TEntity> { Where = entity => batch.Contains(entity.Id), TrackChanges = true }, CancellationToken.None)
				.ConfigureAwait(false);

			foreach (TEntity entity in tracked)
				_ = apply(entity, changed[entity.Id]);

			_ = await repositoryService.CommitChangesAsync(CancellationToken.None).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Deletes the rows that are gone upstream.
	/// </summary>
	private async Task DeleteAsync<TEntity>(
		Func<IRepositoryService, IRepositoryBase<TEntity>> repository,
		IReadOnlyList<TEntity> entities,
		CancellationToken cancellationToken)
		where TEntity : EntityBase
	{
		foreach (int[] batch in entities.Select(entity => entity.Id).Chunk(GetBatchSize()))
		{
			cancellationToken.ThrowIfCancellationRequested();

			using IServiceScope scope = _serviceScopeFactory.CreateScope();
			IRepositoryService repositoryService = GetRepositoryService(scope);

			_ = await repository(repositoryService).ExecuteDeleteAsync(batch, CancellationToken.None).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Deletes the cached files of the logos that are gone upstream.
	/// </summary>
	/// <remarks>
	/// Two logos of the same channel and feed share a file name, so a file a surviving logo still
	/// points to is kept.
	/// </remarks>
	private Task DeleteCachedLogos(IReadOnlyList<LogoEntity> removed, IReadOnlyList<LogoEntity> kept, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		HashSet<string> keptPaths = new(
			kept.Select(logo => logo.LocalPath).OfType<string>(),
			StringComparer.OrdinalIgnoreCase);

		int deleted = 0;
		foreach (string path in removed.Select(logo => logo.LocalPath).OfType<string>().Distinct(StringComparer.OrdinalIgnoreCase))
		{
			if (!keptPaths.Contains(path) && _logoStoreService.Delete(path))
				deleted++;
		}

		if (deleted > 0)
			_eventService.Publish(new LogoCacheChangedEvent(0));

		return Task.CompletedTask;
	}

	/// <summary>
	/// Writes what is known about the list, so the screen can show how old the catalog is.
	/// </summary>
	private async Task WriteStatusAsync(CatalogKindResponse response, CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		DateTime now = _providerService.DateTime.UtcNow;
		CatalogKind kind = response.Kind;

		CatalogSyncEntity? entity = await repositoryService.CatalogSyncs
			.GetSingleAsync(new Query<CatalogSyncEntity> { Where = sync => sync.Kind == kind, TrackChanges = true }, CancellationToken.None)
			.ConfigureAwait(false);

		if (entity is null)
		{
			await repositoryService.CatalogSyncs
				.CreateAsync(
					new CatalogSyncEntity
					{
						Kind = kind,
						FirstImported = now,
						LastChecked = now,
						LastChanged = now,
						Added = response.Added,
						Updated = response.Updated,
						Removed = response.Removed
					},
					CancellationToken.None)
				.ConfigureAwait(false);
		}
		else
		{
			entity.LastChecked = now;
			entity.Added = response.Added;
			entity.Updated = response.Updated;
			entity.Removed = response.Removed;

			if (response.Changes > 0)
				entity.LastChanged = now;
		}

		_ = await repositoryService.CommitChangesAsync(CancellationToken.None).ConfigureAwait(false);
	}

	/// <summary>
	/// Tells the screen and the status bar how far the run is.
	/// </summary>
	private void ReportProgress(CatalogKindResponse response, int completedTasks)
	{
		CatalogSyncProgressEvent progressEvent = new(
			response.Kind, response.Added, response.Updated, response.Removed, completedTasks, TotalSyncTasks);

		_eventService.Publish(progressEvent);

		// The status bar of the main window follows every long running operation, the logo cache does the same.
		_eventService.Publish(new ProgressChangedEvent(progressEvent.ProgressPercentage));
	}

	/// <summary>
	/// Gets the number of rows one statement writes.
	/// </summary>
	private int GetBatchSize()
		=> Math.Max(1, _applicationSettings.Database.MaxBatchSize);

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}
