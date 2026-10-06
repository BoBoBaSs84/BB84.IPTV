// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class GuideServiceTests
{
	[TestMethod]
	public async Task SearchGuidesAsyncShouldReadEveryGuideInOneFixedOrder()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest()).ConfigureAwait(false);

		// By channel, a guide without one first, then by site and site id.
		Assert.AreSequenceEqual(
			["orphan", "ard", "ard-1", "unknown", "zdf"],
			page.Select(guide => guide.SiteId).ToArray());
		Assert.AreEqual(5, page.MetaData.TotalCount);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldLimitTheGuidesToTheSite()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { Site = "tvtoday.de" }).ConfigureAwait(false);

		Assert.AreEqual("ard-1", page.Single().SiteId);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldFindTheIdentifierOfAChannelsXml()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "DasErste.de@SD" }).ConfigureAwait(false);

		Assert.AreEqual("ard-1", page.Single().SiteId);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldFindWhatOnlyTheChannelNameHolds()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "zweites" }).ConfigureAwait(false);

		GuideOptionResponse guide = page.Single();

		Assert.AreEqual("zdf", guide.SiteId);
		Assert.AreEqual("Zweites Programm", guide.ChannelName);
		Assert.AreEqual("DE", guide.Country);
	}

	[TestMethod]
	[DataRow("HOERZU", 4, DisplayName = "the site")]
	[DataRow("ARD-1", 1, DisplayName = "the site id")]
	[DataRow("orphan", 1, DisplayName = "the site name")]
	[DataRow("EN", 1, DisplayName = "the language")]
	[DataRow("zdf.DE", 1, DisplayName = "the channel")]
	public async Task SearchGuidesAsyncShouldIgnoreTheCaseOfAsciiLetters(string text, int expected)
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = text }).ConfigureAwait(false);

		Assert.HasCount(expected, page);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldKeepAGuideTheCatalogDoesNotKnow()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { SearchText = "unknown" }).ConfigureAwait(false);

		GuideOptionResponse guide = page.Single();

		Assert.AreEqual("Unknown.de", guide.Channel);
		Assert.IsNull(guide.ChannelName);
		Assert.IsNull(guide.Country);
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldNotCountAFirstPageThatIsNotFull()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest()).ConfigureAwait(false);

		Assert.AreEqual(5, page.MetaData.TotalCount);
		Assert.AreEqual(0, _countQueries, "The first page holds the whole result, so it is its count.");
	}

	[TestMethod]
	public async Task SearchGuidesAsyncShouldCountTheResultForALaterPage()
	{
		IPagedList<GuideOptionResponse> page = await SearchAsync(new GuideSearchRequest { PageNumber = 2, PageSize = Parameters.MinPageSize })
			.ConfigureAwait(false);

		Assert.IsEmpty(page);
		Assert.AreEqual(5, page.MetaData.TotalCount);
		Assert.AreEqual(1, _countQueries);
		Assert.AreEqual(Parameters.MinPageSize, _guideQueries[^1].Skip);
		Assert.AreEqual(Parameters.MinPageSize, _guideQueries[^1].Take);
	}

	private async Task<IPagedList<GuideOptionResponse>> SearchAsync(GuideSearchRequest request)
		=> await _sut.SearchGuidesAsync(request, TestContext.CancellationToken).ConfigureAwait(false);
}
