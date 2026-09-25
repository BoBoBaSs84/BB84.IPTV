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
	/// Logs the error that was shown to the user.
	/// </summary>
	/// <remarks>
	/// The message is the localized text of the notification, so the entry is written in the
	/// language the user was addressed in.
	/// </remarks>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="message">The message the user was shown.</param>
	/// <param name="exception">The exception the error was reported with.</param>
	[LoggerMessage(LogEvents.Presentation.NotificationError, LogLevel.Error, "{Message}")]
	public static partial void NotificationError(ILogger logger, string message, Exception? exception);

	/// <summary>
	/// Logs the warning that was shown to the user.
	/// </summary>
	/// <remarks>
	/// The message is the localized text of the notification, so the entry is written in the
	/// language the user was addressed in.
	/// </remarks>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="message">The message the user was shown.</param>
	[LoggerMessage(LogEvents.Presentation.NotificationWarning, LogLevel.Warning, "{Message}")]
	public static partial void NotificationWarning(ILogger logger, string message);

	/// <summary>
	/// Logs the information that was shown to the user.
	/// </summary>
	/// <remarks>
	/// The message is the localized text of the notification, so the entry is written in the
	/// language the user was addressed in.
	/// </remarks>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="message">The message the user was shown.</param>
	[LoggerMessage(LogEvents.Presentation.NotificationInformation, LogLevel.Information, "{Message}")]
	public static partial void NotificationInformation(ILogger logger, string message);
}
