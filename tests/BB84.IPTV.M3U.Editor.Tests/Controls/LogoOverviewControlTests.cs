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
public sealed class LogoOverviewControlTests
{
	[TestMethod]
	public async Task ShouldShowTheEntriesAndTheLogosWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			LogoOverviewViewModel viewModel = ViewModelFactory.CreateLogoOverview();
			LogoOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				// The control loads the playlists, the entries and the first page of logos when it
				// gets its data context.
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("LogoDataGrid");

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(2, viewModel.Entries);
				Assert.HasCount(2, viewModel.Logos);
				Assert.AreEqual(2, grid.ItemsSource!.Cast<object>().Count());
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task EverySortableColumnShouldSortByAKnownColumn()
		=> await UiTest.RunAsync(() =>
		{
			LogoOverviewViewModel viewModel = ViewModelFactory.CreateLogoOverview();
			LogoOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("LogoDataGrid");

				Assert.IsTrue(grid.CanUserSortColumns);

				// A column the query cannot order by, like the name of the channel, is not sortable.
				foreach (DataGridColumn column in grid.Columns.Where(column => column.CanUserSort))
				{
					Assert.IsTrue(
						Enum.TryParse(column.SortMemberPath, out LogoSortColumn _),
						$"'{column.SortMemberPath}' is no column the query can order by.");
				}
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task AHeaderClickShouldOrderTheWholeResult()
		=> await UiTest.RunAsync(() =>
		{
			List<LogoSearchRequest> requests = [];
			Mock<ILogoService> logoServiceMock = new();
			LogoOverviewViewModel viewModel = ViewModelFactory.CreateLogoOverview(logoServiceMock);

			// The search of the factory still answers, the requests it gets are the ones asserted on.
			logoServiceMock
				.Setup(x => x.SearchLogosAsync(It.IsAny<LogoSearchRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((LogoSearchRequest request, CancellationToken _) =>
				{
					requests.Add(request);

					return new PagedList<LogoOptionResponse>([], 0, request.PageNumber, request.PageSize);
				});

			LogoOverviewControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				UiTest.Settle();

				ClickColumnHeader(control, Properties.Resources.LogoOverviewControl_DownloadedAtColumn_Header);

				Assert.AreEqual(LogoSortColumn.DownloadedAt, requests[^1].SortBy);
				Assert.IsFalse(requests[^1].Descending);
				Assert.AreEqual(1, requests[^1].PageNumber);

				// The second click turns the order round, which the query has to follow.
				ClickColumnHeader(control, Properties.Resources.LogoOverviewControl_DownloadedAtColumn_Header);

				Assert.AreEqual(LogoSortColumn.DownloadedAt, requests[^1].SortBy);
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
