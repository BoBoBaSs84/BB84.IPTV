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
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class PlaylistControlTests
{
	[TestMethod]
	public async Task ShouldShowThePlaylistWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistViewModel viewModel = CreateEditor();
			PlaylistControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				Assert.HasCount(2, viewModel.Entries);
				Assert.IsTrue(viewModel.CanReorderEntries);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task SortingTheEntriesShouldTurnTheReorderingOffAndKeepThePlaylistOrder()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistViewModel viewModel = CreateEditor();
			PlaylistControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				Button moveUp = FindButton(control, "Move Up");
				Button moveDown = FindButton(control, "Move Down");
				IEntry first = viewModel.Entries[0];

				Assert.IsTrue(moveUp.IsEffectivelyEnabled);
				Assert.IsTrue(moveDown.IsEffectivelyEnabled);

				// Sorted descending, so the view no longer shows the order the playlist is written in.
				ClickColumnHeader(control, "Title");
				ClickColumnHeader(control, "Title");

				Assert.IsTrue(viewModel.IsSorted);
				Assert.IsFalse(viewModel.CanReorderEntries);
				Assert.IsFalse(moveUp.IsEffectivelyEnabled);
				Assert.IsFalse(moveDown.IsEffectivelyEnabled);

				viewModel.SelectedEntry = first;
				viewModel.MoveSelectedEntry(1);

				Assert.AreSame(first, viewModel.Entries[0]);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	private static PlaylistViewModel CreateEditor()
	{
		Mock<IPlaylistService> playlistServiceMock = new();
		playlistServiceMock.Setup(x => x.LoadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new PlaylistModel(new PlaylistModel(),
			[
				new EntryModel("Das Erste", "https://example.com/ard.m3u8"),
				new EntryModel("Local camera", "rtsp://192.168.12.1:554")
			]));

		PlaylistViewModel viewModel = new(playlistServiceMock.Object, new Mock<IFileService>().Object, new Mock<ILogoService>().Object, new Mock<IFileDialogService>().Object, new Mock<IClipboardService>().Object, new Mock<IEventService>().Object);
		viewModel.LoadAsync(1, "Mine", CancellationToken.None).GetAwaiter().GetResult();

		return viewModel;
	}

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

	private static Button FindButton(Control control, string content)
		=> control.GetVisualDescendants()
			.OfType<Button>()
			.First(button => Equals(button.Content, content));
}
