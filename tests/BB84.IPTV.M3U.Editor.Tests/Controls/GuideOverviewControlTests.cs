// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Queries;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
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
	public async Task EveryGridShouldSortAndResizeItsColumns()
		=> await UiTest.RunAsync(() =>
		{
			GuideOverviewViewModel viewModel = ViewModelFactory.CreateGuideOverview();
			GuideOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				UiTest.Settle();

				foreach (DataGrid grid in control.GetVisualDescendants().OfType<DataGrid>())
				{
					Assert.IsTrue(grid.CanUserSortColumns, $"'{grid.Name}' does not sort.");
					Assert.IsTrue(grid.CanUserResizeColumns, $"'{grid.Name}' does not resize.");

					// Only a picture column opts out of the sort.
					foreach (DataGridColumn column in grid.Columns.Where(column => column is not DataGridTemplateColumn))
						Assert.IsTrue(column.CanUserSort, $"'{column.Header}' of '{grid.Name}' does not sort.");
				}
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task AHeaderClickShouldSortThePageWithoutSearchingAgain()
		=> await UiTest.RunAsync(() =>
		{
			List<GuideSearchQuery> queries = [];
			Mock<IGuideService> serviceMock = new();
			GuideOverviewViewModel viewModel = ViewModelFactory.CreateGuideOverview(serviceMock);

			// The search of the factory still answers, the queries it gets are the ones counted.
			serviceMock
				.Setup(x => x.SearchGuidesAsync(It.IsAny<GuideSearchQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((GuideSearchQuery query, CancellationToken _) =>
				{
					queries.Add(query);

					return new PagedList<GuideOptionResponse>([], 0, query.PageNumber, query.PageSize);
				});

			GuideOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("GuideDataGrid");
				int searches = queries.Count;

				ClickColumnHeader(control, Properties.Resources.GuideOverviewControl_ChannelNameColumn_Header);

				// The grid sorts the page it holds, the database is not asked again.
				Assert.HasCount(searches, queries);
				Assert.AreEqual(ListSortDirection.Ascending, grid.CollectionView!.SortDescriptions.Single().Direction);

				ClickColumnHeader(control, Properties.Resources.GuideOverviewControl_ChannelNameColumn_Header);

				Assert.HasCount(searches, queries);
				Assert.AreEqual(ListSortDirection.Descending, grid.CollectionView!.SortDescriptions.Single().Direction);
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
