// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class GuideControlTests
{
	[TestMethod]
	public async Task ShouldShowTheMappingsWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			GuideViewModel viewModel = ViewModelFactory.CreateGuide();
			GuideControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				// The control loads the playlists when it gets its data context.
				UiTest.Settle();

				DataGrid grid = control.GetControl<DataGrid>("MappingDataGrid");

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(2, viewModel.Playlists);
				Assert.HasCount(2, viewModel.Mappings);
				Assert.AreEqual(2, grid.ItemsSource!.Cast<object>().Count());
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheCommandsShouldFollowTheMappings()
		=> await UiTest.RunAsync(() =>
		{
			GuideViewModel viewModel = ViewModelFactory.CreateGuide();
			GuideControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				Button export = control.GetVisualDescendants()
					.OfType<Button>()
					.First(button => Equals(button.Content, Properties.Resources.GuideControl_ExportButton_Content));

				// The control loaded the playlists and mapped the first one already.
				Assert.IsTrue(export.IsEffectivelyEnabled);

				viewModel.SelectedPlaylist = null;
				viewModel.LoadMappingsAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				Assert.IsEmpty(viewModel.Mappings);
				Assert.IsFalse(export.IsEffectivelyEnabled, "Without a playlist there is nothing to export.");
			});
		}).ConfigureAwait(false);
}