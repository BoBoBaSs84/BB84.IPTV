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
/// Looks through the imported guides in a real SQLite database: what a channel can be mapped to and
/// what the sites cover.
/// </summary>
[TestClass]
public sealed class GuideOptionTests
{
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
			new GuideEntity { Channel = "ZDF.de", Feed = null, Site = "example.com", SiteId = "201", SiteName = "ZDF", Lang = "de" },
			new GuideEntity { Channel = "ZDF.de", Feed = "HD", Site = "hd.example.com", SiteId = "200", SiteName = "ZDF HD", Lang = "de" },
			new GuideEntity { Channel = "ZDF.de", Feed = "SD", Site = "sd.example.com", SiteId = "202", SiteName = "ZDF SD", Lang = "de" },
			new GuideEntity { Channel = "DasErste.de", Feed = null, Site = "example.com", SiteId = "100", SiteName = "Das Erste", Lang = "de" }
		];

		// A site with more guides than one page holds, so the paging can be told apart from a full list.
		for (int index = 0; index < 150; index++)
		{
			guides.Add(new GuideEntity
			{
				Channel = $"Channel{index:D3}.de",
				Feed = null,
				Site = "big.example.com",
				SiteId = $"{index:D3}",
				SiteName = $"Channel {index:D3}",
				Lang = index % 2 is 0 ? "de" : "en"
			});
		}

		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Guides.CreateAsync(guides).ConfigureAwait(false);

			await repository.Channels.CreateAsync(
			[
				new ChannelEntity { Channel = "ZDF.de", Name = "ZDF", Country = "DE", Categories = [], AltNames = [], Owners = [] },
				new ChannelEntity { Channel = "Channel000.de", Name = "Channel zero", Country = "AT", Categories = [], AltNames = [], Owners = [] }
			]).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldTellWhatTheCatalogKnows()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "hd.example.com" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page);
		Assert.AreEqual("ZDF", page[0].ChannelName);
		Assert.AreEqual("DE", page[0].Country);
	}

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldLeaveAnUnknownChannelWithoutAName()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "big.example.com", SearchText = "Channel001" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page, "Only Channel001 matches, the catalog does not know it.");
		Assert.IsNull(page[0].ChannelName);
		Assert.IsNull(page[0].Country);
	}

	[TestMethod]
	public async Task GetOptionsAsyncShouldReturnTheGuideOfTheFeedFirst()
	{
		IReadOnlyList<GuideOptionResponse> options = await _sut.GetOptionsAsync("ZDF.de", "HD").ConfigureAwait(false);

		Assert.HasCount(3, options);
		Assert.AreEqual("hd.example.com", options[0].Site);
		Assert.AreEqual("200", options[0].SiteId);

		// The rest keeps the order the prefill uses, by site.
		Assert.AreEqual("example.com", options[1].Site);
		Assert.AreEqual("sd.example.com", options[2].Site);
	}

	[TestMethod]
	public async Task GetOptionsAsyncShouldOrderBySiteWithoutAFeed()
	{
		IReadOnlyList<GuideOptionResponse> options = await _sut.GetOptionsAsync("ZDF.de").ConfigureAwait(false);

		Assert.AreEqual("example.com", options[0].Site);
		Assert.AreEqual("hd.example.com", options[1].Site);
		Assert.AreEqual("sd.example.com", options[2].Site);
	}

	[TestMethod]
	public async Task GetOptionsAsyncShouldBeEmptyForAnUnknownChannel()
	{
		IReadOnlyList<GuideOptionResponse> options = await _sut.GetOptionsAsync("Unknown.de").ConfigureAwait(false);

		Assert.IsEmpty(options);
	}

	[TestMethod]
	public async Task GetSitesAsyncShouldCountWhatASiteCovers()
	{
		IReadOnlyList<GuideSiteResponse> sites = await _sut.GetSitesAsync().ConfigureAwait(false);

		Assert.HasCount(4, sites);
		Assert.AreEqual("big.example.com", sites[0].Site, "The sites are ordered by name.");

		GuideSiteResponse example = sites.Single(site => site.Site == "example.com");

		Assert.AreEqual(2, example.ChannelCount);
		Assert.AreEqual(2, example.GuideCount);
		Assert.AreEqual(150, sites.Single(site => site.Site == "big.example.com").ChannelCount);
	}

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldPageTheGuidesOfTheSite()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "big.example.com", PageNumber = 2, PageSize = 100 })
			.ConfigureAwait(false);

		Assert.HasCount(50, page);
		Assert.AreEqual(150, page.MetaData.TotalCount);
		Assert.AreEqual(2, page.MetaData.TotalPages);
		Assert.AreEqual("Channel100.de", page[0].Channel);
	}

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldFilterBySearchText()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "big.example.com", SearchText = "Channel01" })
			.ConfigureAwait(false);

		Assert.HasCount(10, page, "Channel010 to Channel019 are the ones that match.");
		Assert.IsTrue(page.All(option => option.Site == "big.example.com"));
	}

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldFindTheSiteName()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "hd.example.com", SearchText = "ZDF HD" })
			.ConfigureAwait(false);

		Assert.HasCount(1, page);
		Assert.AreEqual("ZDF.de", page[0].Channel);
		Assert.AreEqual("HD", page[0].Feed);
	}

	[TestMethod]
	public async Task SearchSiteChannelsAsyncShouldBeEmptyForAnUnknownSite()
	{
		IPagedList<GuideOptionResponse> page = await _sut
			.SearchSiteChannelsAsync(new GuideSiteSearchRequest { Site = "nowhere.example.com" })
			.ConfigureAwait(false);

		Assert.IsEmpty(page);
	}
}
