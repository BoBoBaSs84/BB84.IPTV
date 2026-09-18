// Copyright: 2025 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Windows;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Extensions;
using BB84.IPTV.M3U.Editor.Presentation.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RESX = BB84.IPTV.M3U.Editor.Properties.Resources;
using WpfApp = System.Windows.Application;

namespace BB84.IPTV.M3U.Editor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : WpfApp
{
	private readonly IHost _host;
	private readonly IEventService _eventService;
	private readonly ILoggerService<App> _loggerService;

	private static readonly Action<ILogger, string, Exception?> LogInformation =
		LoggerMessage.Define<string>(LogLevel.Information, 0, "{Information}");

	private static readonly Action<ILogger, Exception?> LogCritical =
		LoggerMessage.Define(LogLevel.Critical, 0, string.Empty);

	/// <summary>
	/// Initializes a new instance of the app class.
	/// </summary>
	public App()
	{
		_host = CreateHostBuilder().Build();

		_eventService = _host.Services.GetRequiredService<IEventService>();
		_loggerService = _host.Services.GetRequiredService<ILoggerService<App>>();

		RegisterEventHandlers();
	}

	private async void Application_Startup(object sender, StartupEventArgs e)
	{
		_loggerService.Log(LogInformation, RESX.ApplicationIsStarting);

		await _host.StartAsync().ConfigureAwait(false);

		MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

	private async void Application_Exit(object sender, ExitEventArgs e)
	{
		_loggerService.Log(LogInformation, RESX.ApplicationIsExiting);

		using (_host)
			await _host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
	}

	private void RegisterEventHandlers()
	{
		DispatcherUnhandledException += (s, e) => OnUnhandledException(e.Exception);
		_eventService.Subscribe<RestartRequestedEvent>(OnRestartRequested);
		_eventService.Subscribe<ExitRequestedEvent>(OnExitRequested);
	}

	private void OnExitRequested(ExitRequestedEvent @event)
	{
		_loggerService.Log(LogInformation, RESX.ExitRequested);
		Current.Shutdown();
	}

	private void OnRestartRequested(RestartRequestedEvent @event)
	{
		_loggerService.Log(LogInformation, RESX.RestartRequested);
		System.Diagnostics.Process.Start(Environment.ProcessPath!);
		Current.Shutdown();
	}

	private void OnUnhandledException(Exception exception)
		=> _loggerService.Log(LogCritical, exception);

	private static IHostBuilder CreateHostBuilder()
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
			.RegisterAppSettingsConfiguration()
			.ConfigureServices((context, services) => services.RegisterServices(context.HostingEnvironment));

		return hostBuilder;
	}
}
