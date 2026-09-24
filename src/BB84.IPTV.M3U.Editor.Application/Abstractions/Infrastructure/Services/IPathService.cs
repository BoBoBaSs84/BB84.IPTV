// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents the file system locations the application works with.
/// </summary>
/// <remarks>
/// The locations are resolved once, while the application starts, from the <c>Paths</c> settings
/// section and the per-user defaults. A path the settings do not name, or name in a way the file
/// system does not take, falls back to its default, so a bad setting never keeps the application
/// from starting.
/// </remarks>
public interface IPathService
{
	/// <summary>
	/// The directory that holds the database.
	/// </summary>
	string DataDirectory { get; }

	/// <summary>
	/// The full path of the SQLite database file.
	/// </summary>
	string DatabaseFilePath { get; }

	/// <summary>
	/// The full path of the settings file, always the default location.
	/// </summary>
	string SettingsFilePath { get; }

	/// <summary>
	/// The directory that holds the cached channel logos, one folder per iptv-org channel.
	/// </summary>
	string LogoDirectory { get; }

	/// <summary>
	/// The directory that holds the log files.
	/// </summary>
	string LogDirectory { get; }

	/// <summary>
	/// Indicates whether the paths the settings name differ from the ones in use, which takes
	/// a restart to apply.
	/// </summary>
	/// <returns><see langword="true"/> if a restart is needed to use the configured paths.</returns>
	bool HasPendingChanges();

	/// <summary>
	/// Indicates whether <paramref name="path"/> can be used as a data directory, which means it
	/// is a rooted path the application may create and write to.
	/// </summary>
	/// <param name="path">The path to check, an empty path stands for the default and is valid.</param>
	/// <returns><see langword="true"/> if the path can be used.</returns>
	bool IsValidDirectory(string? path);
}