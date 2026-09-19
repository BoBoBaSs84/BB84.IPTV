// Copyright: 2025 Robert Peter Meyer
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
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Extensions;
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
	private ILoggerService<App>? _loggerService;

	private static readonly Action<ILogger, string, Exception?> LogInformation =
		LoggerMessage.Define<string>(LogLevel.Information, 0, "{Information}");

	private static readonly Action<ILogger, string, Exception?> LogError =
		LoggerMessage.Define<string>(LogLevel.Error, 0, "{Error}");

	private static readonly Action<ILogger, Exception?> LogCritical =
		LoggerMessage.Define(LogLevel.Critical, 0, string.Empty);

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
			_loggerService = _host.Services.GetRequiredService<ILoggerService<App>>();

			RegisterEventHandlers(desktop);
		}

		base.OnFrameworkInitializationCompleted();
	}

	private void RegisterEventHandlers(IClassicDesktopStyleApplicationLifetime desktop)
	{
		desktop.Startup += async (s, e) => await OnStartupAsync(desktop).ConfigureAwait(true);
		desktop.Exit += (s, e) => OnExit();
		Dispatcher.UIThread.UnhandledException += (s, e) => OnUnhandledException(e.Exception);
		_eventService!.Subscribe<RestartRequestedEvent>(e => OnRestartRequested(desktop));
		_eventService.Subscribe<ExitRequestedEvent>(e => OnExitRequested(desktop));
	}

	private async Task OnStartupAsync(IClassicDesktopStyleApplicationLifetime desktop)
	{
		_loggerService!.Log(LogInformation, RESX.ApplicationIsStarting);

		await _host!.StartAsync().ConfigureAwait(true);

		// Resolved before the settings are loaded, so that load errors are shown to the user.
		_ = _host.Services.GetRequiredService<INotificationService>();

		await _host.Services.GetRequiredService<ISettingsService>()
			.LoadAsync()
			.ConfigureAwait(true);

		ApplyLanguage(_host.Services.GetRequiredService<ApplicationSettings>().General.Language);

		await MigrateDatabaseAsync().ConfigureAwait(true);
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
			// The app still starts, the database view can be used to create the database again.
			_loggerService!.Log(LogError, RESX.DatabaseMigrationFailed, ex);
			_eventService!.Publish(new ErrorOccuredEvent(RESX.DatabaseMigrationFailed, ex));
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
			_loggerService!.Log(LogError, ex.Message, ex);
			_eventService!.Publish(new ErrorOccuredEvent(ex.Message, ex));
		}
	}

	private void OnExit()
	{
		_loggerService?.Log(LogInformation, RESX.ApplicationIsExiting);

		if (_host is null)
			return;

		using (_host)
			_host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
	}

	private void OnExitRequested(IClassicDesktopStyleApplicationLifetime desktop)
	{
		_loggerService!.Log(LogInformation, RESX.ExitRequested);

		// Closing the main window asks about unsaved changes and ends the application.
		if (desktop.MainWindow is { } mainWindow)
			mainWindow.Close();
		else
			desktop.Shutdown();
	}

	private void OnRestartRequested(IClassicDesktopStyleApplicationLifetime desktop)
	{
		_loggerService!.Log(LogInformation, RESX.RestartRequested);
		Process.Start(Environment.ProcessPath!, desktop.Args ?? []);
		desktop.Shutdown();
	}

	private void OnUnhandledException(Exception exception)
		=> _loggerService?.Log(LogCritical, exception);

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