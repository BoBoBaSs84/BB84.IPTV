// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Queries;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class GuideOverviewViewModelTests : IDisposable
{
	private const int TotalGuides = 250;

	private readonly Mock<IGuideService> _guideServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly List<GuideSearchQuery> _queries = [];
	private readonly GuideOverviewViewModel _sut;

	public GuideOverviewViewModelTests()
	{
		_guideServiceMock.Setup(x => x.GetSitesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideSiteResponse { Site = "hoerzu.de", ChannelCount = 2, GuideCount = 4 },
				new GuideSiteResponse { Site = "magentatv.de", ChannelCount = 1, GuideCount = 1 },
				new GuideSiteResponse { Site = "tvtoday.de", ChannelCount = 1, GuideCount = 1 }
			]);

		_guideServiceMock.Setup(x => x.SearchGuidesAsync(It.IsAny<GuideSearchQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((GuideSearchQuery query, CancellationToken _) =>
			{
				_queries.Add(query);

				return new PagedList<GuideOptionResponse>(
					[new GuideOptionResponse { Channel = "DasErste.de", Site = "hoerzu.de", SiteId = "ard", SiteName = "ARD", Lang = "de" }],
					TotalGuides,
					query.PageNumber,
					query.PageSize);
			});

		_sut = new GuideOverviewViewModel(_guideServiceMock.Object, _eventServiceMock.Object);
	}

	/// <summary>
	/// The view model releases the source of a search that is still running when it is disposed.
	/// </summary>
	public void Dispose()
		=> _sut.Dispose();

	[TestMethod]
	public async Task LoadAndReportAsyncShouldShowTheProvidersAndTheFirstPage()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		Assert.HasCount(3, _sut.Providers);
		Assert.HasCount(1, _sut.Guides);
		Assert.AreEqual(1, _sut.PageNumber);
		Assert.AreEqual(TotalGuides, _sut.TotalCount);
		Assert.IsTrue(_sut.HasNextPage);
		Assert.IsFalse(_sut.HasPreviousPage);
		Assert.IsFalse(_sut.IsBusy);

		GuideSearchQuery query = _queries.Single();

		Assert.AreEqual(PagedQuery.MinPageSize, query.PageSize);
		Assert.IsNull(query.Site);
	}

	[TestMethod]
	public async Task LoadProvidersAsyncShouldReadTheProvidersOnlyOnce()
	{
		await _sut.LoadProvidersAsync(cancellationToken: TestContext.CancellationToken).ConfigureAwait(false);
		await _sut.LoadProvidersAsync(cancellationToken: TestContext.CancellationToken).ConfigureAwait(false);
		await _sut.LoadProvidersAsync(true, TestContext.CancellationToken).ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.GetSitesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[TestMethod]
	public async Task SearchCommandShouldPassTheTextAndStartAtTheFirstPage()
	{
		await _sut.LoadAsync(3).ConfigureAwait(false);
		_sut.SearchText = "Das Erste";

		await _sut.SearchCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);

		GuideSearchQuery query = _queries[^1];

		Assert.AreEqual("Das Erste", query.SearchText);
		Assert.AreEqual(1, query.PageNumber);
		Assert.AreEqual(1, _sut.PageNumber);
	}

	[TestMethod]
	public async Task SelectingAProviderShouldLimitTheSearchToIt()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_sut.SelectedProvider = _sut.Providers[1];
		await WaitForQueriesAsync(2).ConfigureAwait(false);

		Assert.AreEqual("magentatv.de", _queries[^1].Site);
		Assert.IsTrue(_sut.ClearProviderCommand.CanExecute());

		_sut.ClearProviderCommand.Execute();
		await WaitForQueriesAsync(3).ConfigureAwait(false);

		Assert.IsNull(_queries[^1].Site);
		Assert.IsNull(_sut.SelectedProvider);
	}

	[TestMethod]
	public async Task TheProviderFilterShouldKeepTheMatchesWithoutAnotherSearch()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_sut.ProviderSearchText = "TV";

		Assert.HasCount(2, _sut.Providers);
		Assert.AreEqual("magentatv.de", _sut.Providers[0].Site);
		Assert.HasCount(1, _queries);

		_sut.ProviderSearchText = string.Empty;

		Assert.HasCount(3, _sut.Providers);
		Assert.HasCount(1, _queries);
	}

	[TestMethod]
	public async Task PagingCommandsShouldFollowThePages()
	{
		Assert.IsFalse(_sut.PreviousPageCommand.CanExecute());
		Assert.IsFalse(_sut.NextPageCommand.CanExecute());

		await _sut.LoadAsync(2).ConfigureAwait(false);

		Assert.IsTrue(_sut.PreviousPageCommand.CanExecute());
		Assert.IsTrue(_sut.NextPageCommand.CanExecute());
	}

	[TestMethod]
	public async Task AFailingSearchShouldBeReported()
	{
		_guideServiceMock.Setup(x => x.SearchGuidesAsync(It.IsAny<GuideSearchQuery>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no database"));

		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
		Assert.IsFalse(_sut.IsBusy);
	}

	/// <summary>
	/// Waits for the searches a property setter starts, which cannot be awaited by the caller.
	/// </summary>
	private async Task WaitForQueriesAsync(int count)
	{
		for (int attempt = 0; attempt < 50 && _queries.Count < count; attempt++)
			await Task.Delay(10, TestContext.CancellationToken).ConfigureAwait(false);

		Assert.HasCount(count, _queries);
	}

	public TestContext TestContext { get; set; } = default!;
}
