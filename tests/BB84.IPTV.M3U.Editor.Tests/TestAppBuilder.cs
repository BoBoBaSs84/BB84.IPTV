// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia;
using Avalonia.Headless;

using BB84.IPTV.M3U.Editor;

[assembly: AvaloniaTestApplication(typeof(BB84.IPTV.M3U.Editor.Tests.TestAppBuilder))]

namespace BB84.IPTV.M3U.Editor.Tests;

/// <summary>
/// Builds the real application for the headless test session.
/// </summary>
/// <remarks>
/// The headless lifetime is not a desktop lifetime, so <see cref="App"/> does not build its host:
/// the views, styles and data templates are loaded, the database and the settings are not touched.
/// </remarks>
public static class TestAppBuilder
{
	/// <summary>
	/// Configures the application for the headless platform.
	/// </summary>
	/// <returns>The configured application builder.</returns>
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}