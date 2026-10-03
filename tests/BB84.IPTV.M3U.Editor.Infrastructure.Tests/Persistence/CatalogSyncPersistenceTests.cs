// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Synchronizes the catalog against a real SQLite database: what is added, updated, removed and
/// left alone, and what is written about the run.
/// </summary>
[TestClass]
public sealed class CatalogSyncPersistenceTests
{
	private SqliteTestDatabase _database = default!;
	private FakeWebService _webService = default!;
	private RecordingLogoStoreService _logoStore = default!;
	private IDatabaseService _sut = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_webService = new FakeWebService();
		_logoStore = new RecordingLogoStoreService();

		_database = SqliteTestDatabase.InMemory(services =>
		{
			services.RemoveAll<IWebService>();
			services.AddSingleton<IWebService>(_webService);
			services.RemoveAll<ILogoStoreService>();
			services.AddSingleton<ILogoStoreService>(_logoStore);
		});

		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		_sut = _database.Services.GetRequiredService<IDatabaseService>();
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task TheFirstRunShouldAddEverythingTheCatalogHolds()
	{
		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);

		Assert.IsTrue(response.IsSuccess);
		Assert.IsTrue(response.HasChanges);
		Assert.AreEqual(0, response.TotalUpdated);
		Assert.AreEqual(0, response.TotalRemoved);

		IReadOnlyList<ChannelEntity> channels = await ReadChannelsAsync().ConfigureAwait(false);

		Assert.HasCount(2, channels);
		Assert.AreEqual("Das Erste", channels.Single(channel => channel.Channel == "DasErste.de").Name);
	}

	[TestMethod]
	public async Task ASecondRunWithoutAChangeShouldChangeNothing()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);

		// A collection column is stored as null when it is empty, which must not read as a change.
		Assert.IsFalse(response.HasChanges);
		Assert.AreEqual(0, response.TotalAdded);
		Assert.AreEqual(0, response.TotalUpdated);
		Assert.AreEqual(0, response.TotalRemoved);
		Assert.AreEqual(2, response.Kinds.Single(kind => kind.Kind is CatalogKind.Channel).Unchanged);
	}

	[TestMethod]
	public async Task AChangedRecordShouldBeUpdatedInPlace()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);
		int storedId = (await ReadChannelsAsync().ConfigureAwait(false))
			.Single(channel => channel.Channel == "DasErste.de").Id;

		_webService.Channels[0] = _webService.Channels[0] with { Name = "Das Erste HD", Categories = ["news", "general"] };

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);
		ChannelEntity updated = (await ReadChannelsAsync().ConfigureAwait(false))
			.Single(channel => channel.Channel == "DasErste.de");

		Assert.AreEqual(1, response.TotalUpdated);
		Assert.AreEqual(0, response.TotalAdded);
		Assert.AreEqual(storedId, updated.Id, "An update keeps the row, so nothing that points at it breaks.");
		Assert.AreEqual("Das Erste HD", updated.Name);
		Assert.HasCount(2, updated.Categories);
	}

	[TestMethod]
	public async Task ARecordThatIsGoneUpstreamShouldBeRemoved()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);
		_webService.Channels.RemoveAt(1);

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);
		IReadOnlyList<ChannelEntity> channels = await ReadChannelsAsync().ConfigureAwait(false);

		Assert.AreEqual(1, response.TotalRemoved);
		Assert.HasCount(1, channels);
		Assert.AreEqual("DasErste.de", channels[0].Channel);
	}

	[TestMethod]
	public async Task AListThatComesBackEmptyShouldBeLeftAlone()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);

		// A failed request looks exactly like this, so the rows have to survive it.
		_webService.Channels.Clear();

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);
		CatalogKindResponse channels = response.Kinds.Single(kind => kind.Kind is CatalogKind.Channel);

		Assert.AreEqual(CatalogSyncOutcome.Skipped, channels.Outcome);
		Assert.IsFalse(response.IsSuccess);
		Assert.HasCount(2, await ReadChannelsAsync().ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ADuplicateOfTheImportShouldBeStoredOnce()
	{
		_webService.Guides.Add(_webService.Guides[0]);

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);
		CatalogKindResponse guides = response.Kinds.Single(kind => kind.Kind is CatalogKind.Guide);

		IReadOnlyList<GuideEntity> stored = await _database
			.WithRepositoryAsync(repository => repository.Guides.GetListAsync(new Query<GuideEntity>()))
			.ConfigureAwait(false);

		Assert.AreEqual(1, guides.Duplicates);
		Assert.HasCount(2, stored);
	}

	[TestMethod]
	public async Task AnUpdatedLogoShouldKeepItsCache()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);
		await CacheLogosAsync().ConfigureAwait(false);

		_webService.Logos[0] = _webService.Logos[0] with { Width = 512 };

		CatalogSyncResponse response = await SynchronizeAsync().ConfigureAwait(false);
		IReadOnlyList<LogoEntity> logos = await ReadLogosAsync().ConfigureAwait(false);
		LogoEntity logo = logos.Single(entity => entity.Url == _webService.Logos[0].Url);

		Assert.AreEqual(1, response.TotalUpdated);
		Assert.AreEqual(512f, logo.Width);
		Assert.AreEqual("etag", logo.ETag, "The columns of the logo cache belong to the application, not to iptv-org.");
		Assert.IsNotNull(logo.LocalPath);
	}

	[TestMethod]
	public async Task ARemovedLogoShouldTakeItsCachedFileWithIt()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);
		await CacheLogosAsync().ConfigureAwait(false);

		string removedUrl = _webService.Logos[1].Url;
		string removedPath = CachePathOf(removedUrl);
		_webService.Logos.RemoveAt(1);

		_ = await SynchronizeAsync().ConfigureAwait(false);

		Assert.Contains(removedPath, _logoStore.Deleted);
		Assert.DoesNotContain(CachePathOf(_webService.Logos[0].Url), _logoStore.Deleted);
	}

	[TestMethod]
	public async Task TheStatusShouldTellWhenTheCatalogWasReadAndChanged()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);

		IReadOnlyList<CatalogStatusResponse> afterFirst = await GetStatusAsync().ConfigureAwait(false);
		CatalogStatusResponse firstRun = afterFirst.Single(status => status.Kind is CatalogKind.Channel);

		_ = await SynchronizeAsync().ConfigureAwait(false);

		IReadOnlyList<CatalogStatusResponse> afterSecond = await GetStatusAsync().ConfigureAwait(false);
		CatalogStatusResponse secondRun = afterSecond.Single(status => status.Kind is CatalogKind.Channel);

		Assert.HasCount(Enum.GetValues<CatalogKind>().Length, afterSecond);
		Assert.IsTrue(firstRun.IsImported);
		Assert.AreEqual(firstRun.FirstImported, secondRun.FirstImported, "The first import is written once.");
		Assert.IsTrue(secondRun.LastChecked >= firstRun.LastChecked);

		// Nothing changed in the second run, so the change stays where it was.
		Assert.AreEqual(firstRun.LastChanged, secondRun.LastChanged);
		Assert.AreEqual(0, secondRun.Added);
	}

	[TestMethod]
	public async Task ResettingTheCatalogShouldForgetWhatWasSynchronized()
	{
		_ = await SynchronizeAsync().ConfigureAwait(false);

		_ = await _sut.CreateDatabaseAsync(TestContext.CancellationToken).ConfigureAwait(false);

		IReadOnlyList<CatalogStatusResponse> statuses = await GetStatusAsync().ConfigureAwait(false);

		Assert.IsTrue(statuses.All(status => !status.IsImported));
		Assert.IsEmpty(await ReadChannelsAsync().ConfigureAwait(false));
	}

	private Task<CatalogSyncResponse> SynchronizeAsync()
		=> _sut.SynchronizeAsync(TestContext.CancellationToken);

	private Task<IReadOnlyList<CatalogStatusResponse>> GetStatusAsync()
		=> _sut.GetCatalogStatusAsync(TestContext.CancellationToken);

	private Task<IReadOnlyList<ChannelEntity>> ReadChannelsAsync()
		=> _database.WithRepositoryAsync(repository => repository.Channels.GetListAsync(new Query<ChannelEntity>()));

	private Task<IReadOnlyList<LogoEntity>> ReadLogosAsync()
		=> _database.WithRepositoryAsync(repository => repository.Logos.GetListAsync(new Query<LogoEntity>()));

	/// <summary>
	/// Pretends the logos were downloaded, so the cache columns hold something to keep or to clean.
	/// </summary>
	private async Task CacheLogosAsync()
		=> await _database.WithRepositoryAsync(async repository =>
		{
			IReadOnlyList<LogoEntity> logos = await repository.Logos
				.GetListAsync(new Query<LogoEntity> { TrackChanges = true })
				.ConfigureAwait(false);

			foreach (LogoEntity logo in logos)
			{
				logo.LocalPath = CachePathOf(logo.Url);
				logo.ETag = "etag";
				logo.DownloadedAt = DateTime.UtcNow;
			}

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);

	private static string CachePathOf(string url)
		=> Path.Combine(Path.GetTempPath(), $"{url.GetHashCode(StringComparison.Ordinal)}.png");

	public TestContext TestContext { get; set; } = default!;

	/// <summary>
	/// Answers with the lists a test puts in, so a run can add, change and drop records.
	/// </summary>
	private sealed class FakeWebService : IWebService
	{
		public List<CategoryRequest> Categories { get; } =
			[new CategoryRequest("news", "News", "What happens.")];

		public List<CountryRequest> Countries { get; } =
			[new CountryRequest("Germany", "DE", ["deu"], "flag")];

		public List<LanguageRequest> Languages { get; } =
			[new LanguageRequest("German", "deu")];

		public List<ChannelRequest> Channels { get; } =
		[
			new ChannelRequest("DasErste.de", "Das Erste", [], null, [], "DE", ["news"], false, null, null, null, null),
			new ChannelRequest("ZDF.de", "ZDF", [], null, [], "DE", [], false, null, null, null, null)
		];

		public List<FeedRequest> Feeds { get; } =
			[new FeedRequest("DasErste.de", "SD", "Standard", [], true, ["DE"], ["Europe/Berlin"], ["deu"], "576i")];

		public List<GuideRequest> Guides { get; } =
		[
			new GuideRequest("DasErste.de", null, "hoerzu.de", "ard", "ARD", "de"),
			new GuideRequest("ZDF.de", null, "hoerzu.de", "zdf", "ZDF", "de")
		];

		public List<LogoRequest> Logos { get; } =
		[
			new LogoRequest("DasErste.de", null, [], 256, 256, "png", "https://example.com/ard.png"),
			new LogoRequest("ZDF.de", null, [], 256, 256, "png", "https://example.com/zdf.png")
		];

		public List<StreamRequest> Streams { get; } =
			[new StreamRequest("DasErste.de", null, "Das Erste", "https://example.com/ard.m3u8", null, null, "1080p")];

		public Task<IEnumerable<CategoryRequest>> GetCategoriesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<CategoryRequest>>([.. Categories]);

		public Task<IEnumerable<CountryRequest>> GetCountriesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<CountryRequest>>([.. Countries]);

		public Task<IEnumerable<LanguageRequest>> GetLanguagesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<LanguageRequest>>([.. Languages]);

		public Task<IEnumerable<ChannelRequest>> GetChannelsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<ChannelRequest>>([.. Channels]);

		public Task<IEnumerable<FeedRequest>> GetFeedsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<FeedRequest>>([.. Feeds]);

		public Task<IEnumerable<GuideRequest>> GetGuidesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<GuideRequest>>([.. Guides]);

		public Task<IEnumerable<LogoRequest>> GetLogosAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<LogoRequest>>([.. Logos]);

		public Task<IEnumerable<StreamRequest>> GetStreamsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<StreamRequest>>([.. Streams]);

		public Task<IEnumerable<BlocklistRequest>> GetBlocklistsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<BlocklistRequest>>([]);

		public Task<IEnumerable<CityRequest>> GetCitiesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<CityRequest>>([]);

		public Task<IEnumerable<RegionRequest>> GetRegionsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<RegionRequest>>([]);

		public Task<IEnumerable<SubdivisionRequest>> GetSubdivisionsAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<SubdivisionRequest>>([]);

		public Task<IEnumerable<TimezoneRequest>> GetTimezonesAsync(CancellationToken cancellationToken = default)
			=> Task.FromResult<IEnumerable<TimezoneRequest>>([]);
	}

	/// <summary>
	/// Writes down which cached files the synchronization asked to delete.
	/// </summary>
	private sealed class RecordingLogoStoreService : ILogoStoreService
	{
		public List<string> Deleted { get; } = [];

		public Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default)
			=> throw new NotSupportedException("The test database does not store logos.");

		public bool Exists(string? path)
			=> !string.IsNullOrWhiteSpace(path);

		public bool Delete(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			Deleted.Add(path);

			return true;
		}

		public int Clear()
			=> 0;
	}
}
