// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia;

namespace BB84.IPTV.M3U.Editor;

/// <summary>
/// The application entry point.
/// </summary>
internal static class Program
{
	/// <summary>
	/// Starts the application.
	/// </summary>
	/// <remarks>
	/// Do not use Avalonia, third-party APIs or any <see cref="SynchronizationContext"/>-reliant
	/// code before <c>StartWithClassicDesktopLifetime</c> is called, nothing is initialized yet.
	/// </remarks>
	/// <param name="args">The command line arguments.</param>
	/// <returns>The exit code of the application.</returns>
	[STAThread]
	public static int Main(string[] args)
		=> BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

	/// <summary>
	/// Configures the Avalonia application, also used by the visual designer.
	/// </summary>
	/// <returns>The configured application builder.</returns>
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
#if DEBUG
			.WithDeveloperTools()
#endif
			.WithInterFont()
			.LogToTrace();
}