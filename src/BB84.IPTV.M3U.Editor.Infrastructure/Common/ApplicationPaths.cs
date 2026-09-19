// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Infrastructure.Common;

/// <summary>
/// Provides the per-user file system locations used by the application.
/// </summary>
/// <remarks>
/// The data directory resolves to <c>%LOCALAPPDATA%</c> on Windows, <c>~/.local/share</c> on Linux
/// and <c>~/Library/Application Support</c> on macOS, each followed by the product name.
/// </remarks>
internal static class ApplicationPaths
{
	/// <summary>
	/// The per-user directory that holds the database, logs and other application data.
	/// </summary>
	internal static string DataDirectory { get; } = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
		AssemblyInformation.Product);

	/// <summary>
	/// The full path of the SQLite database file.
	/// </summary>
	internal static string DatabaseFilePath { get; } = Path.Combine(DataDirectory, $"{AssemblyInformation.Product}.db");

	/// <summary>
	/// The full path of the application settings file.
	/// </summary>
	internal static string SettingsFilePath { get; } = Path.Combine(DataDirectory, $"{AssemblyInformation.Product}.ini");

	/// <summary>
	/// The directory that holds the log files.
	/// </summary>
	internal static string LogDirectory { get; } = Path.Combine(DataDirectory, "logs");
}