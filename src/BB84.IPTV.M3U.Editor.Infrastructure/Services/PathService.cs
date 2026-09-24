// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the file system locations the application works with, resolved from the <c>Paths</c>
/// settings section and the per-user defaults.
/// </summary>
/// <remarks>
/// The locations are resolved in the constructor, so the database, the logger and the logo store
/// keep the paths they were set up with for as long as the application runs. A changed setting is
/// used after a restart, which <see cref="HasPendingChanges"/> reports.
/// </remarks>
internal sealed class PathService : IPathService
{
	private static readonly StringComparison PathComparison =
		OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

	private readonly PathSettings _settings;

	/// <summary>
	/// Initializes a new instance of the <see cref="PathService"/> class.
	/// </summary>
	/// <param name="applicationSettings">The current application settings.</param>
	public PathService(ApplicationSettings applicationSettings)
	{
		ArgumentNullException.ThrowIfNull(applicationSettings);

		_settings = applicationSettings.Paths;

		DataDirectory = Resolve(_settings.DataDirectory, ApplicationPaths.DataDirectory);
		DatabaseFilePath = Path.Combine(DataDirectory, ApplicationPaths.DatabaseFileName);
		SettingsFilePath = ApplicationPaths.SettingsFilePath;
		LogoDirectory = Resolve(_settings.LogoDirectory, Path.Combine(DataDirectory, ApplicationPaths.LogoDirectoryName));
		LogDirectory = Resolve(_settings.LogDirectory, Path.Combine(DataDirectory, ApplicationPaths.LogDirectoryName));
	}

	/// <inheritdoc/>
	public string DataDirectory { get; }

	/// <inheritdoc/>
	public string DatabaseFilePath { get; }

	/// <inheritdoc/>
	public string SettingsFilePath { get; }

	/// <inheritdoc/>
	public string LogoDirectory { get; }

	/// <inheritdoc/>
	public string LogDirectory { get; }

	/// <inheritdoc/>
	public bool HasPendingChanges()
	{
		string dataDirectory = Resolve(_settings.DataDirectory, ApplicationPaths.DataDirectory);
		string logoDirectory = Resolve(_settings.LogoDirectory, Path.Combine(dataDirectory, ApplicationPaths.LogoDirectoryName));
		string logDirectory = Resolve(_settings.LogDirectory, Path.Combine(dataDirectory, ApplicationPaths.LogDirectoryName));

		return !dataDirectory.Equals(DataDirectory, PathComparison)
			|| !logoDirectory.Equals(LogoDirectory, PathComparison)
			|| !logDirectory.Equals(LogDirectory, PathComparison);
	}

	/// <inheritdoc/>
	public bool IsValidDirectory(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return true;

		try
		{
			return Path.IsPathFullyQualified(path)
				&& path.IndexOfAny(Path.GetInvalidPathChars()) < 0
				&& Path.GetFullPath(path).Length > 0;
		}
		catch (Exception ex) when (ex is ArgumentException or PathTooLongException or NotSupportedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Turns a configured path into the absolute path to use, falling back to <paramref name="fallback"/>
	/// for a path that is empty or that the file system does not take.
	/// </summary>
	/// <param name="path">The configured path.</param>
	/// <param name="fallback">The default path to use instead.</param>
	/// <returns>The absolute path without a trailing directory separator.</returns>
	private string Resolve(string? path, string fallback)
	{
		string resolved = IsValidDirectory(path) && !string.IsNullOrWhiteSpace(path)
			? Path.GetFullPath(path.Trim())
			: fallback;

		return Path.TrimEndingDirectorySeparator(resolved);
	}
}