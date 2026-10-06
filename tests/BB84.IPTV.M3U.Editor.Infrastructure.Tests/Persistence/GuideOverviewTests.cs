// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Searches the imported guides of every site in a real SQLite database, as the guide overview
/// screen does.
/// </summary>
/// <remarks>
/// The seed holds 250 guides on <c>big.example.com</c> whose channel names run against their
/// identifiers, so an order that only covers the page that is read can be told apart from one the
/// database applies to the whole result.
/// </remarks>
[TestClass]
public sealed class GuideOverviewTests
{
	private const int SeededChannels = 250;

	private SqliteTestDatabase _database = default!;
	private IGuideService _sut = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		_sut = _database.Services.GetRequiredService<IGuideService>();

		List<GuideEntity> guides =
		[
			new GuideEntity { Channel = "DasErste.de", Feed = null, Site = "hoerzu.de", SiteId = "ard", SiteName = "ARD", Lang = "de" },
			new GuideEntity { Channel = "DasErste.de", Feed = "SD", Site = "tvtoday.de", SiteId = "ard-1", SiteName = "Das Erste", Lang = "de" },
			new GuideEntity { Channel = "DasErste.de", Feed = "HD", Site = "magentatv.de", SiteId = "ard-hd", SiteName = "Das Erste HD", Lang = "de" },
			new GuideEntity { Channel = "ZDF.de", Feed = null, Site = "hoerzu.de", SiteId = "zdf", SiteName = "ZDF", Lang = "de" },
			// A guide of a channel the catalog does not know, and one that names no channel at all.
			new GuideEntity { Channel = "Unknown.de", Feed = null, Site = "hoerzu.de", SiteId = "unknown", SiteName = "Unknown station", Lang = "de" },
			new GuideEntity { Channel = null, Feed = null, Site = "hoerzu.de", SiteId = "orphan", SiteName = "Orphan", Lang = "de" }
		];

		List<ChannelEntity> channels =
		[
			new ChannelEntity { Channel = "DasErste.de", Name = "Das Erste", Country = "DE", Categories = [], AltNames = [], Owners = [] },
			new ChannelEntity { Channel = "ZDF.de", Name = "ZDF", Country = "DE", Categories = [], AltNames = [], Owners = [] }
		];

		// The channel name counts down while the identifier counts up, so the two orders differ.
		for (int index = 0; index < SeededChannels; index++)
		{
			string channel = $"Channel{index:D3}.de";

			guides.Add(new GuideEntity
			{
				Channel = channel,
				Feed = null,
				Site = "big.example.com",
				SiteId = $"{index:D3}",
				SiteName = $"Channel {index:D3}",
				Lang = index % 2 is 0 ? "de" : "en"
			});

			channels.Add(new ChannelEntity
			{
				Channel = channel,
				Name = $"Station {SeededChannels - 1 - index:D3}",
				Country = index % 2 is 0 ? "DE" : "AT",
				Categories = [],
				AltNames = [],
				Owners = []
			});
		}

		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Guides.CreateAsync(guides).ConfigureAwait(false);
			await repository.Channels.CreateAsync(channels).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task SearchGuidesAsyncShouldTellWhichProvidersCarryTheChannel()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "DasErste.de" })
			.ConfigureAwait(false);

		Assert.HasCount(3, page);
		Assert.AreEqual(3, page.MetaData.TotalCount);
		Assert.AreEqual(1, page.MetaData.TotalPages);
		CollectionAssert.AreEquivalent(
			new List<string> { "hoerzu.de", "magentatv.de", "tvtoday.de" },
			page.Select(guide => guide.Site).ToList());
		Assert.IsTrue(page.All(guide => guide.ChannelName is "Das Erste" && guide.Country is "DE"));
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldFindWhatOnlyTheChannelNameHolds()
	{
		// "Station" exists in the catalog channels only, so without the join nothing is found.
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "Station 249" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page);
		Assert.AreEqual("Channel000.de", page[0].Channel);
		Assert.AreEqual("Station 249", page[0].ChannelName);
	}

	[TestMethod]
	[DataRow("hoerzu.de", 4, DisplayName = "the provider")]
	[DataRow("ard-hd", 1, DisplayName = "the site identifier")]
	[DataRow("Das Erste HD", 1, DisplayName = "the site name")]
	[DataRow("Unknown.de", 1, DisplayName = "the channel identifier")]
	public async Task SearchGuidesAsyncShouldMatchEveryColumn(string searchText, int expected)
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = searchText })
			.ConfigureAwait(false);

		Assert.AreEqual(expected, page.MetaData.TotalCount);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldMatchTheLanguage()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "en" })
			.ConfigureAwait(false);

		// Every guide the language matches, plus the ones holding "en" somewhere else.
		Assert.IsTrue(page.MetaData.TotalCount >= SeededChannels / 2);
		Assert.IsTrue(page.All(guide
			=> guide.Lang.Contains("en", StringComparison.OrdinalIgnoreCase)
			|| guide.Site.Contains("en", StringComparison.OrdinalIgnoreCase)
			|| guide.SiteId.Contains("en", StringComparison.OrdinalIgnoreCase)
			|| guide.SiteName.Contains("en", StringComparison.OrdinalIgnoreCase)
			|| (guide.Channel?.Contains("en", StringComparison.OrdinalIgnoreCase) ?? false)
			|| (guide.ChannelName?.Contains("en", StringComparison.OrdinalIgnoreCase) ?? false)));
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldIgnoreTheCaseOfAsciiLetters()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "ard" })
			.ConfigureAwait(false);

		Assert.AreEqual(3, page.MetaData.TotalCount);
		Assert.IsTrue(page.Any(guide => guide.SiteName is "ARD"));
	}

	[TestMethod]
	[DataRow("_")]
	[DataRow("%")]
	public async Task SearchGuidesAsyncShouldTakeTheMeaningOffAWildcard(string searchText)
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = searchText })
			.ConfigureAwait(false);

		Assert.AreEqual(0, page.MetaData.TotalCount);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldFindTheIdentifierOfAChannelsXml()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "DasErste.de@SD" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page);
		Assert.AreEqual("SD", page[0].Feed);
		Assert.AreEqual("tvtoday.de", page[0].Site);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldKeepAGuideTheCatalogDoesNotKnow()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "Orphan" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page);
		Assert.IsNull(page[0].Channel);
		Assert.IsNull(page[0].ChannelName);
		Assert.IsNull(page[0].Country);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldLimitTheGuidesToTheProvider()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { Site = "hoerzu.de" })
			.ConfigureAwait(false);

		Assert.AreEqual(4, page.MetaData.TotalCount);
		Assert.IsTrue(page.All(guide => guide.Site is "hoerzu.de"));
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldPageTheWholeResultInOneFixedOrder()
	{
		GuideSearchRequest request = new()
		{
			Site = "big.example.com",
			PageNumber = 1,
			PageSize = Parameters.MinPageSize
		};

		IPagedList<GuideOptionResponse> first = await SearchAsync(request).ConfigureAwait(false);

		Assert.AreEqual(SeededChannels, first.MetaData.TotalCount);
		Assert.AreEqual(3, first.MetaData.TotalPages);

		// The channel identifier orders the guides, the grid only sorts the page it shows.
		Assert.AreEqual("Channel000.de", first[0].Channel);

		List<string> channels = [];
		for (int pageNumber = 1; pageNumber <= first.MetaData.TotalPages; pageNumber++)
		{
			IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest
			{
				Site = request.Site,
				PageNumber = pageNumber,
				PageSize = request.PageSize
			}).ConfigureAwait(false);

			channels.AddRange(page.Select(guide => guide.Channel!));
		}

		// Every guide once, in the order the database applied.
		Assert.HasCount(SeededChannels, channels);
		Assert.HasCount(SeededChannels, channels.Distinct(StringComparer.Ordinal).ToList());
		CollectionAssert.AreEqual(channels.OrderBy(channel => channel, StringComparer.Ordinal).ToList(), channels);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldPageGuidesThatShareTheColumn()
	{
		// Every guide of the site holds the same language, so only the tie breaker keeps the paging apart.
		List<string> siteIds = [];

		for (int pageNumber = 1; pageNumber <= 2; pageNumber++)
		{
			IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest
			{
				Site = "big.example.com",
				SearchText = "de",
				PageNumber = pageNumber,
				PageSize = Parameters.MinPageSize
			}).ConfigureAwait(false);

			siteIds.AddRange(page.Select(guide => guide.SiteId));
		}

		Assert.HasCount(siteIds.Count, siteIds.Distinct(StringComparer.Ordinal).ToList());
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldReturnAnEmptyPageWhenNothingMatches()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "nothing holds this" })
			.ConfigureAwait(false);

		Assert.IsEmpty(page);
		Assert.AreEqual(0, page.MetaData.TotalCount);
		Assert.AreEqual(0, page.MetaData.TotalPages);
	}

	private Task<IPagedList<GuideOptionResponse>> SearchAsync(GuideSearchRequest request)
		=> _sut.SearchGuidesAsync(request, TestContext.CancellationToken);

	public TestContext TestContext { get; set; } = default!;
}
