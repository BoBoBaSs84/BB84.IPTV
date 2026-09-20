// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;
using BB84.IPTV.M3U.Editor.Views;

using Microsoft.Extensions.Hosting;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Views;

[TestClass]
public sealed class MainWindowTests
{
	[TestMethod]
	public async Task ShouldShowThePlaylistScreenWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			using BindingErrorSink sink = new();
			(MainWindow window, INavigationService navigationService, _) = CreateWindow();

			try
			{
				UiTest.ShowWindow(window);
				navigationService.NavigateTo<PlaylistsViewModel>();
				UiTest.Settle();

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(1, window.GetVisualDescendants().OfType<PlaylistsControl>().ToList());
			}
			finally
			{
				window.Close();
			}
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task EveryScreenShouldHaveItsControl()
		=> await UiTest.RunAsync(() =>
		{
			using BindingErrorSink sink = new();
			(MainWindow window, INavigationService navigationService, _) = CreateWindow();

			try
			{
				UiTest.ShowWindow(window);

				navigationService.NavigateTo<CatalogViewModel>();
				UiTest.Settle();
				Assert.HasCount(1, window.GetVisualDescendants().OfType<CatalogControl>().ToList());

				navigationService.NavigateTo<MergeViewModel>();
				UiTest.Settle();
				Assert.HasCount(1, window.GetVisualDescendants().OfType<MergeControl>().ToList());

				navigationService.NavigateTo<PlaylistsViewModel>();
				UiTest.Settle();
				Assert.HasCount(1, window.GetVisualDescendants().OfType<PlaylistsControl>().ToList());

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			}
			finally
			{
				window.Close();
			}
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheFileMenuShouldUseThePlaylistCommands()
		=> await UiTest.RunAsync(() =>
		{
			(MainWindow window, _, PlaylistsViewModel playlists) = CreateWindow();

			try
			{
				UiTest.ShowWindow(window);

				Assert.AreSame(playlists.NewCommand, window.GetControl<MenuItem>("NewMenuItem").Command);
				Assert.AreSame(playlists.ImportCommand, window.GetControl<MenuItem>("ImportMenuItem").Command);
				Assert.AreSame(playlists.SaveCommand, window.GetControl<MenuItem>("SaveMenuItem").Command);
				Assert.AreSame(playlists.ExportCommand, window.GetControl<MenuItem>("ExportMenuItem").Command);
				Assert.AreSame(playlists.MergeCommand, window.GetControl<MenuItem>("MergeMenuItem").Command);
			}
			finally
			{
				window.Close();
			}
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheSaveMenuItemShouldFollowTheEditor()
		=> await UiTest.RunAsync(() =>
		{
			(MainWindow window, _, PlaylistsViewModel playlists) = CreateWindow();

			try
			{
				UiTest.ShowWindow(window);
				MenuItem save = window.GetControl<MenuItem>("SaveMenuItem");

				// The File menu is opened first, like a user does, so the items take their command state.
				window.GetControl<MenuItem>("FileMenuItem").IsSubMenuOpen = true;
				UiTest.Settle();

				Assert.IsFalse(playlists.SaveCommand.CanExecute());
				Assert.IsFalse(save.IsEffectivelyEnabled, "Nothing is open, so there is nothing to save.");

				playlists.LoadPlaylistsAsync().GetAwaiter().GetResult();
				playlists.OpenAsync(playlists.Playlists[0]).GetAwaiter().GetResult();
				playlists.Editor.Name = "Changed";
				UiTest.Settle();

				Assert.IsTrue(playlists.SaveCommand.CanExecute());
				Assert.IsTrue(save.IsEffectivelyEnabled);
			}
			finally
			{
				window.Close();
			}
		}).ConfigureAwait(false);

	/// <summary>
	/// Creates the main window with the real navigation service, so the data templates are used.
	/// </summary>
	private static (MainWindow Window, INavigationService NavigationService, PlaylistsViewModel Playlists) CreateWindow()
	{
		PlaylistsViewModel playlists = ViewModelFactory.CreatePlaylists();
		CatalogViewModel catalog = ViewModelFactory.CreateCatalog();
		MergeViewModel merge = ViewModelFactory.CreateMerge();

		TestNavigationService navigationService = new(new Dictionary<Type, ViewModelBase>
		{
			[typeof(PlaylistsViewModel)] = playlists,
			[typeof(CatalogViewModel)] = catalog,
			[typeof(MergeViewModel)] = merge
		});

		Mock<IHostEnvironment> hostEnvironmentMock = new();
		hostEnvironmentMock.SetupGet(x => x.ApplicationName).Returns("BB84.IPTV.M3U.Editor");
		hostEnvironmentMock.SetupGet(x => x.EnvironmentName).Returns("Tests");

		Mock<IUserService> userServiceMock = new();
		userServiceMock.SetupGet(x => x.Domain).Returns("domain");
		userServiceMock.SetupGet(x => x.Name).Returns("user");
		userServiceMock.SetupGet(x => x.Machine).Returns("machine");

		MainViewModel mainViewModel = new(
			new Mock<IEventService>().Object,
			hostEnvironmentMock.Object,
			new Mock<INotificationService>().Object,
			userServiceMock.Object,
			navigationService);

		return (new MainWindow(navigationService, mainViewModel, playlists), navigationService, playlists);
	}
}