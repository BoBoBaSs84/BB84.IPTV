// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class LogoServiceTests
{
	[TestMethod]
	public async Task SearchLogosAsyncShouldReadEveryLogoWithWhatTheCatalogKnowsAboutItsChannel()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest(), TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(3, logos, "Every logo of a channel is offered, not only the one that is picked for it.");
		Assert.AreEqual(3, logos.MetaData.TotalCount);

		LogoOptionResponse first = logos[0];

		Assert.AreEqual("DasErste.de", first.Channel);
		Assert.AreEqual("Das Erste", first.ChannelName, "The channel of the page is read for the row.");
		Assert.AreEqual("DE", first.Country);
		Assert.AreEqual(string.Empty, first.Tags);
		Assert.AreEqual("dark", logos[1].Tags);

		// The catalog does not know the channel of the third logo, which is kept without a name.
		Assert.IsNull(logos[2].ChannelName);
		Assert.IsNull(logos[2].Country);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldPageAndOrderInTheDatabase()
	{
		IPagedList<LogoOptionResponse> page = await _sut
			.SearchLogosAsync(
				new LogoSearchRequest { SortBy = LogoSortColumn.Url, Descending = true, PageNumber = 2, PageSize = Parameters.MinPageSize },
				TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsEmpty(page, "The three logos fit on the first page.");
		Assert.AreEqual(3, page.MetaData.TotalCount, "The count covers the whole result, not the page.");

		// The order, the page and the count are the query, not a list the service sorted itself.
		Query<LogoEntity> query = _queries[^1];

		Assert.IsNotNull(query.OrderBy);
		Assert.AreEqual(Parameters.MinPageSize, query.Skip);
		Assert.AreEqual(Parameters.MinPageSize, query.Take);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldKeepOnlyWhatTheCacheFilterAsksFor()
	{
		_logos[0].LocalPath = Path.Combine("logos", "DasErste.de", "DasErste.de-1.png");

		IPagedList<LogoOptionResponse> cached = await _sut
			.SearchLogosAsync(new LogoSearchRequest { CacheState = LogoCacheFilter.Cached }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		IPagedList<LogoOptionResponse> missing = await _sut
			.SearchLogosAsync(new LogoSearchRequest { CacheState = LogoCacheFilter.NotCached }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(1, cached);
		Assert.HasCount(2, missing);
		Assert.IsFalse(cached[0].IsCached, "The row holds a path, but the store holds no file.");
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldAskTheStoreWhetherTheFileOfTheRowIsThere()
	{
		_ = await _sut.CacheLogosAsync(cancellationToken: TestContext.CancellationToken).ConfigureAwait(false);

		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest(), TestContext.CancellationToken)
			.ConfigureAwait(false);

		// The run cached one logo per channel, the tagged one of the first channel stayed behind.
		Assert.IsTrue(logos.First(logo => logo.Id is 1).IsCached);
		Assert.IsFalse(logos.First(logo => logo.Id is 2).IsCached);
		Assert.IsTrue(logos.First(logo => logo.Id is 3).IsCached);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldFindALogoByItsTextAndByTheNameOfItsChannel()
	{
		Assert.HasCount(2, await SearchAsync("DasErste").ConfigureAwait(false), "The identifier of the channel.");
		Assert.HasCount(2, await SearchAsync("Das Erste").ConfigureAwait(false), "The name the catalog knows the channel under.");
		Assert.HasCount(1, await SearchAsync("zdf.svg").ConfigureAwait(false), "The URL.");
		Assert.HasCount(1, await SearchAsync("SVG").ConfigureAwait(false), "The format.");
		Assert.IsEmpty(await SearchAsync("nothing").ConfigureAwait(false));
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldReadEveryLogoOfTheChannelOfATvgId()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest { SearchText = "DasErste.de@HD" }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		// The feed only decides which logo is preferred, so the whole channel is offered.
		Assert.HasCount(2, logos);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldLimitTheResultToOneChannel()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest { Channel = " ZDF.de " }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(1, logos, "The channel is trimmed before it is matched.");
		Assert.AreEqual("ZDF.de", logos[0].Channel);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldThrowWhenThereIsNoRequest()
		=> _ = await Assert.ThrowsExactlyAsync<ArgumentNullException>(
			() => _sut.SearchLogosAsync(null!, TestContext.CancellationToken)).ConfigureAwait(false);

	private async Task<IPagedList<LogoOptionResponse>> SearchAsync(string searchText)
		=> await _sut
			.SearchLogosAsync(new LogoSearchRequest { SearchText = searchText }, TestContext.CancellationToken)
			.ConfigureAwait(false);
}
