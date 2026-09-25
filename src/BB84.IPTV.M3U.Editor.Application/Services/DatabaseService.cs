// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the database service, which is responsible for managing the application's data storage and retrieval.
/// </summary>
/// <param name="eventService">The event service, for subscribing and publishing events related to database operations.</param>
/// <param name="serviceScopeFactory">The scope factory used to resolve scoped infrastructure services.</param>
internal sealed class DatabaseService(IEventService eventService, IServiceScopeFactory serviceScopeFactory) : IDatabaseService
{
	private const int TotalImportTasks = 8;
	private readonly IEventService _eventService = eventService;
	private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
	private int _completedTasks;

	public async Task<bool> CheckDatabaseAvailabilityAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		return await repositoryService.CanConnectAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();

		await repositoryService.MigrateDatabaseAsync(cancellationToken).ConfigureAwait(false);
		await repositoryService.ResetCatalogAsync(cancellationToken).ConfigureAwait(false);

		return true;
	}

	public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		await repositoryService.MigrateDatabaseAsync(cancellationToken).ConfigureAwait(false);
	}

	public async Task<DatabaseImportResponse> ImportDatabaseAsync(CancellationToken cancellationToken = default)
	{
		_completedTasks = 0;

		int categoriesImported = await ImportWithProgress("Categories", ImportCategories(cancellationToken)).ConfigureAwait(false);
		int countriesImported = await ImportWithProgress("Countries", ImportCountries(cancellationToken)).ConfigureAwait(false);
		int languagesImported = await ImportWithProgress("Languages", ImportLanguages(cancellationToken)).ConfigureAwait(false);
		int channelsImported = await ImportWithProgress("Channels", ImportChannels(cancellationToken)).ConfigureAwait(false);
		int feedsImported = await ImportWithProgress("Feeds", ImportFeeds(cancellationToken)).ConfigureAwait(false);
		int guidesImported = await ImportWithProgress("Guides", ImportGuides(cancellationToken)).ConfigureAwait(false);
		int logosImported = await ImportWithProgress("Logos", ImportLogos(cancellationToken)).ConfigureAwait(false);
		int streamsImported = await ImportWithProgress("Streams", ImportStreams(cancellationToken)).ConfigureAwait(false);

		return new DatabaseImportResponse
		{
			CategoriesImported = categoriesImported,
			CountriesImported = countriesImported,
			LanguagesImported = languagesImported,
			ChannelsImported = channelsImported,
			FeedsImported = feedsImported,
			GuidesImported = guidesImported,
			LogosImported = logosImported,
			StreamsImported = streamsImported
		};
	}

	private async Task<int> ImportWithProgress(string repositoryName, Task<int> importTask)
	{
		int recordsImported = await importTask.ConfigureAwait(false);
		int completed = Interlocked.Increment(ref _completedTasks);

		DatabaseImportProgressEvent progressEvent = new(repositoryName, recordsImported, completed, TotalImportTasks);

		_eventService.Publish(progressEvent);

		// The status bar of the main window follows every long running operation, the logo cache does the same.
		_eventService.Publish(new ProgressChangedEvent(progressEvent.ProgressPercentage));

		return recordsImported;
	}

	private async Task<int> ImportCategories(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<CategoryRequest> result = await webService
			.GetCategoriesAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<CategoryEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Categories
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportCountries(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<CountryRequest> result = await webService
			.GetCountriesAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<CountryEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Countries
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportLanguages(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<LanguageRequest> result = await webService
			.GetLanguagesAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<LanguageEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Languages
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportChannels(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<ChannelRequest> result = await webService
			.GetChannelsAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<ChannelEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Channels
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportFeeds(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<FeedRequest> result = await webService
			.GetFeedsAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<FeedEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Feeds
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportGuides(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<GuideRequest> result = await webService
			.GetGuidesAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<GuideEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Guides
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportLogos(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<LogoRequest> result = await webService
			.GetLogosAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<LogoEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Logos
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	private async Task<int> ImportStreams(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = scope.ServiceProvider.GetRequiredService<IRepositoryService>();
		IWebService webService = scope.ServiceProvider.GetRequiredService<IWebService>();

		IEnumerable<StreamRequest> result = await webService
			.GetStreamsAsync(cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<StreamEntity> entities = result.Select(x => x.ToEntity());

		await repositoryService.Streams
			.CreateAsync(entities, cancellationToken)
			.ConfigureAwait(false);

		return await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);
	}
}
