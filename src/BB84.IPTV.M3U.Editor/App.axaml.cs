// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Extensions;
using BB84.IPTV.M3U.Editor.Services;
using BB84.IPTV.M3U.Editor.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AvaloniaApp = Avalonia.Application;
using RESX = BB84.IPTV.M3U.Editor.Properties.Resources;

namespace BB84.IPTV.M3U.Editor;

/// <summary>
/// Interaction logic for App.axaml
/// </summary>
public partial class App : AvaloniaApp
{
	private IHost? _host;
	private IEventService? _eventService;
	private ILogger<App>? _logger;

	/// <inheritdoc/>
	public override void Initialize()
		=> AvaloniaXamlLoader.Load(this);

	/// <inheritdoc/>
	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			_host = CreateHostBuilder(desktop.Args ?? []).Build();
			_eventService = _host.Services.GetRequiredService<IEventService>();
			_logger = _host.Services.GetRequiredService<ILogger<App>>();

			RegisterEventHandlers(desktop);
		}

		base.OnFrameworkInitializationCompleted();
	}

	private void RegisterEventHandlers(IClassicDesktopStyleApplicationLifetime desktop)
	{
		desktop.Startup += async (s, e) => await OnStartupAsync(desktop).ConfigureAwait(true);
		desktop.Exit += (s, e) => OnExit();
		Dispatcher.UIThread.UnhandledException += (s, e) => Log.UnhandledException(_logger!, e.Exception);
		_eventService!.Subscribe<RestartRequestedEvent>(e => OnRestartRequested(desktop));
		_eventService.Subscribe<ExitRequestedEvent>(e => OnExitRequested(desktop));
	}

	private async Task OnStartupAsync(IClassicDesktopStyleApplicationLifetime desktop)
	{
		Log.ApplicationStarting(_logger!);

		await _host!.StartAsync().ConfigureAwait(true);

		// Resolved before the settings are loaded, so that load errors are shown to the user.
		_ = _host.Services.GetRequiredService<INotificationService>();

		await _host.Services.GetRequiredService<ISettingsService>()
			.LoadAsync()
			.ConfigureAwait(true);

		ApplyLanguage(_host.Services.GetRequiredService<ApplicationSettings>().General.Language);

		await MigrateDatabaseAsync().ConfigureAwait(true);
		await LoadLogoCacheAsync().ConfigureAwait(true);
		await ShowPlaylistsAsync().ConfigureAwait(true);

		MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
		desktop.MainWindow = mainWindow;
		mainWindow.Show();
	}

	private async Task MigrateDatabaseAsync()
	{
		try
		{
			await _host!.Services.GetRequiredService<IDatabaseService>()
				.MigrateDatabaseAsync()
				.ConfigureAwait(true);
		}
		catch (Exception ex)
		{
			// The app still starts, the database view can be used to create the database again. The
			// notification service logs what it shows, so the message is published only.
			_eventService!.Publish(new ErrorOccuredEvent(RESX.DatabaseMigrationFailed, ex));
		}
	}

	/// <summary>
	/// Reads which logos are cached, so the views show them without asking the network.
	/// </summary>
	private async Task LoadLogoCacheAsync()
	{
		LogoImageService logoImageService = _host!.Services.GetRequiredService<LogoImageService>();
		LogoImageService.Current = logoImageService;

		try
		{
			await logoImageService.RefreshAsync().ConfigureAwait(true);
		}
		catch (Exception ex)
		{
			// The logos are a convenience, the application runs without them.
			Log.LogoCacheRefreshFailed(_logger!, ex);
		}
	}

	private async Task ShowPlaylistsAsync()
	{
		PlaylistsViewModel playlistsViewModel = _host!.Services.GetRequiredService<PlaylistsViewModel>();
		_host.Services.GetRequiredService<INavigationService>().NavigateTo<PlaylistsViewModel>();

		try
		{
			await playlistsViewModel.LoadPlaylistsAsync().ConfigureAwait(true);
		}
		catch (Exception ex)
		{
			// The notification service logs what it shows, so the message is published only.
			_eventService!.Publish(new ErrorOccuredEvent(ex.Message, ex));
		}
	}

	private void OnExit()
	{
		if (_logger is not null)
			Log.ApplicationExiting(_logger);

		if (_host is null)
			return;

		using (_host)
			_host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
	}

	private void OnExitRequested(IClassicDesktopStyleApplicationLifetime desktop)
	{
		Log.ApplicationExitRequested(_logger!);

		// Closing the main window asks about unsaved changes and ends the application.
		if (desktop.MainWindow is { } mainWindow)
			mainWindow.Close();
		else
			desktop.Shutdown();
	}

	private void OnRestartRequested(IClassicDesktopStyleApplicationLifetime desktop)
	{
		Log.ApplicationRestartRequested(_logger!);
		Process.Start(Environment.ProcessPath!, desktop.Args ?? []);
		desktop.Shutdown();
	}

	private static void ApplyLanguage(Language language)
	{
		string cultureName = typeof(Language)
			.GetField(language.ToString())?
			.GetCustomAttribute<DescriptionAttribute>()?
			.Description ?? "en-US";

		CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
			.RegisterAppSettingsConfiguration()
			.ConfigureServices((context, services) => services.RegisterServices(context.HostingEnvironment));

		return hostBuilder;
	}
}
