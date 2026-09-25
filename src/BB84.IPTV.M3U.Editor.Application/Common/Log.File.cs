// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Application.Common;

public static partial class Log
{
	/// <summary>
	/// Logs that a playlist is being loaded from a file.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="filePath">The path of the file the playlist is read from.</param>
	[LoggerMessage(LogEvents.File.PlaylistLoading, LogLevel.Information, "Loading the playlist from '{FilePath}'.")]
	public static partial void PlaylistFileLoading(ILogger logger, string filePath);

	/// <summary>
	/// Logs that a playlist was loaded from a file.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="filePath">The path of the file the playlist was read from.</param>
	[LoggerMessage(LogEvents.File.PlaylistLoaded, LogLevel.Information, "Loaded the playlist from '{FilePath}'.")]
	public static partial void PlaylistFileLoaded(ILogger logger, string filePath);

	/// <summary>
	/// Logs that a playlist is being saved to a file.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="filePath">The path of the file the playlist is written to.</param>
	[LoggerMessage(LogEvents.File.PlaylistSaving, LogLevel.Information, "Saving the playlist to '{FilePath}'.")]
	public static partial void PlaylistFileSaving(ILogger logger, string filePath);

	/// <summary>
	/// Logs that a playlist was saved to a file.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="filePath">The path of the file the playlist was written to.</param>
	[LoggerMessage(LogEvents.File.PlaylistSaved, LogLevel.Information, "Saved the playlist to '{FilePath}'.")]
	public static partial void PlaylistFileSaved(ILogger logger, string filePath);
}
