// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class GuideOverviewControlTests
{
	[TestMethod]
	public async Task ShouldShowTheGuidesWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			GuideOverviewViewModel viewModel = ViewModelFactory.CreateGuideOverview();
			GuideOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				// The control loads the providers and the first page when it gets its data context.
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("GuideDataGrid");

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(3, viewModel.Providers);
				Assert.HasCount(2, viewModel.Guides);
				Assert.AreEqual(2, grid.ItemsSource!.Cast<object>().Count());
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task EveryColumnShouldSortByAKnownColumn()
		=> await UiTest.RunAsync(() =>
		{
			GuideOverviewViewModel viewModel = ViewModelFactory.CreateGuideOverview();
			GuideOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("GuideDataGrid");

				Assert.IsTrue(grid.CanUserSortColumns);

				foreach (DataGridColumn column in grid.Columns)
				{
					Assert.IsTrue(
						Enum.TryParse(column.SortMemberPath, out GuideSortColumn _),
						$"'{column.SortMemberPath}' is no column the query can order by.");
				}
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task AHeaderClickShouldOrderTheWholeResult()
		=> await UiTest.RunAsync(() =>
		{
			List<GuideSearchRequest> requests = [];
			Mock<IGuideService> guideServiceMock = new();
			GuideOverviewViewModel viewModel = ViewModelFactory.CreateGuideOverview(guideServiceMock);

			// The search of the factory still answers, the requests it gets are the ones asserted on.
			guideServiceMock
				.Setup(x => x.SearchGuidesAsync(It.IsAny<GuideSearchRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((GuideSearchRequest request, CancellationToken _) =>
				{
					requests.Add(request);

					return new PagedList<GuideOptionResponse>([], 0, request.PageNumber, request.PageSize);
				});

			GuideOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				UiTest.Settle();

				ClickColumnHeader(control, Properties.Resources.GuideOverviewControl_ChannelNameColumn_Header);

				Assert.AreEqual(GuideSortColumn.ChannelName, requests[^1].SortBy);
				Assert.IsFalse(requests[^1].Descending);
				Assert.AreEqual(1, requests[^1].PageNumber);

				// The second click turns the order round, which the query has to follow.
				ClickColumnHeader(control, Properties.Resources.GuideOverviewControl_ChannelNameColumn_Header);

				Assert.AreEqual(GuideSortColumn.ChannelName, requests[^1].SortBy);
				Assert.IsTrue(requests[^1].Descending);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	private static void ClickColumnHeader(Control control, string header)
	{
		Window window = (Window)TopLevel.GetTopLevel(control)!;
		DataGridColumnHeader columnHeader = control.GetVisualDescendants()
			.OfType<DataGridColumnHeader>()
			.First(candidate => Equals(candidate.Content, header));

		Point point = columnHeader.TranslatePoint(columnHeader.Bounds.Center - columnHeader.Bounds.Position, window)
			?? throw new InvalidOperationException("The column header is not shown.");

		window.MouseDown(point, MouseButton.Left);
		window.MouseUp(point, MouseButton.Left);
		UiTest.Settle();
	}
}
