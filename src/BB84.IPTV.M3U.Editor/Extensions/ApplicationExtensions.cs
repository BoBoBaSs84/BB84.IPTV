using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using AvaloniaApp = Avalonia.Application;

namespace BB84.IPTV.M3U.Editor.Extensions;

/// <summary>
/// Provides access to the windows of the running Avalonia application.
/// </summary>
internal static class ApplicationExtensions
{
	/// <summary>
	/// Gets the window that currently has the focus, or the main window if none is active.
	/// </summary>
	/// <returns>The active window, or <see langword="null"/> if no window is open.</returns>
	internal static Window? GetActiveWindow()
	{
		if (AvaloniaApp.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
			return null;

		return desktop.Windows.FirstOrDefault(window => window.IsActive) ?? desktop.MainWindow;
	}
}