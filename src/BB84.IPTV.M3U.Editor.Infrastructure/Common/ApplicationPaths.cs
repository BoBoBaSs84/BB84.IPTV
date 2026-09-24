// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Infrastructure.Common;

/// <summary>
/// Provides the default per-user file system locations used by the application.
/// </summary>
/// <remarks>
/// The data directory resolves to <c>%LOCALAPPDATA%</c> on Windows, <c>~/.local/share</c> on Linux
/// and <c>~/Library/Application Support</c> on macOS, each followed by the product name. The
/// <see cref="Services.PathService"/> turns these defaults and the <c>Paths</c> settings section
/// into the locations the application works with.
/// </remarks>
internal static class ApplicationPaths
{
	/// <summary>
	/// The name of the directory that holds the cached channel logos.
	/// </summary>
	internal const string LogoDirectoryName = "logos";

	/// <summary>
	/// The name of the directory that holds the log files.
	/// </summary>
	internal const string LogDirectoryName = "logs";

	/// <summary>
	/// The per-user directory that holds the database, logs and other application data.
	/// </summary>
	internal static string DataDirectory { get; } = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create),
		AssemblyInformation.Product);

	/// <summary>
	/// The name of the SQLite database file.
	/// </summary>
	internal static string DatabaseFileName { get; } = $"{AssemblyInformation.Product}.db";

	/// <summary>
	/// The name of the application settings file.
	/// </summary>
	internal static string SettingsFileName { get; } = $"{AssemblyInformation.Product}.ini";

	/// <summary>
	/// The full path of the SQLite database file.
	/// </summary>
	internal static string DatabaseFilePath { get; } = Path.Combine(DataDirectory, DatabaseFileName);

	/// <summary>
	/// The full path of the application settings file.
	/// </summary>
	/// <remarks>
	/// The settings file stays here, it is what tells the application where everything else lives.
	/// </remarks>
	internal static string SettingsFilePath { get; } = Path.Combine(DataDirectory, SettingsFileName);

	/// <summary>
	/// The directory that holds the cached channel logos, one folder per iptv-org channel.
	/// </summary>
	internal static string LogoDirectory { get; } = Path.Combine(DataDirectory, LogoDirectoryName);

	/// <summary>
	/// The directory that holds the log files.
	/// </summary>
	internal static string LogDirectory { get; } = Path.Combine(DataDirectory, LogDirectoryName);
}