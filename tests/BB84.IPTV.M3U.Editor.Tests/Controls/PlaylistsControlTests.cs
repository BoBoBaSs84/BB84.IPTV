// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class PlaylistsControlTests
{
	[TestMethod]
	public async Task ShouldShowTheStoredPlaylistsWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists();
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();

			PlaylistsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				ListBox list = control.GetControl<ListBox>("PlaylistList");

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.AreEqual(2, list.ItemCount);
				Assert.IsNull(list.SelectedItem);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task SelectingAPlaylistShouldOpenItInTheEditor()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists();
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();

			PlaylistsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				ListBox list = control.GetControl<ListBox>("PlaylistList");

				list.SelectedIndex = 1;
				UiTest.Settle();

				Assert.AreEqual(2, viewModel.CurrentPlaylist?.Id);
				Assert.IsTrue(viewModel.Editor.HasPlaylist);
				Assert.AreSame(viewModel.CurrentPlaylist, list.SelectedItem);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task CancellingTheSaveQuestionShouldKeepTheOpenPlaylistSelected()
		=> await UiTest.RunAsync(() =>
		{
			Mock<INotificationService> notificationServiceMock = new();

			// A closed dialog cancels, so the other playlist must not be opened.
			notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.Cancel);

			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists(notificationServiceMock: notificationServiceMock);
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();

			PlaylistsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				ListBox list = control.GetControl<ListBox>("PlaylistList");

				list.SelectedIndex = 0;
				UiTest.Settle();

				// An edit makes the open playlist dirty, so switching asks first.
				viewModel.Editor.Name = "First changed";

				list.SelectedIndex = 1;
				UiTest.Settle();

				Assert.AreEqual(1, viewModel.CurrentPlaylist?.Id);
				Assert.AreSame(viewModel.Playlists[0], list.SelectedItem);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task ThePagingShouldBeHiddenForASinglePage()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists();
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();

			PlaylistsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				Assert.AreEqual(1, viewModel.TotalPages);
				Assert.IsFalse(viewModel.HasMultiplePages);
				Assert.IsFalse(control.GetVisualDescendants().OfType<TextBlock>().Any(block => block.Text == viewModel.PageStatus && block.IsEffectivelyVisible));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheEditorShouldShowTheEntriesOfTheOpenPlaylist()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists();
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();
			viewModel.OpenAsync(viewModel.Playlists[0]).GetAwaiter().GetResult();

			PlaylistControl editor = new() { DataContext = viewModel.Editor };

			UiTest.InWindow(editor, sink =>
			{
				DataGrid grid = editor.GetControl<DataGrid>("EntriesDataGrid");

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(2, viewModel.Editor.Entries);
				Assert.AreEqual(2, grid.ItemsSource!.Cast<object>().Count());
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheEditorShouldShowWhyThePlaylistCannotBeSaved()
		=> await UiTest.RunAsync(() =>
		{
			PlaylistsViewModel viewModel = ViewModelFactory.CreatePlaylists();
			viewModel.LoadPlaylistsAsync().GetAwaiter().GetResult();
			viewModel.OpenAsync(viewModel.Playlists[0]).GetAwaiter().GetResult();

			PlaylistControl editor = new() { DataContext = viewModel.Editor };

			UiTest.InWindow(editor, _ =>
			{
				Assert.IsTrue(viewModel.Editor.IsValid);

				viewModel.Editor.AddEntry();
				UiTest.Settle();

				// The new entry has no URL, so the reason is shown.
				Assert.IsFalse(viewModel.Editor.IsValid);
				Assert.IsTrue(editor.GetVisualDescendants().OfType<TextBlock>()
					.Any(block => block.Text == viewModel.Editor.ValidationMessage && block.IsEffectivelyVisible));
			});
		}).ConfigureAwait(false);
}