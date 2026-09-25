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
	/// Logs that the application is starting.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	[LoggerMessage(LogEvents.Application.Starting, LogLevel.Information, "The application is starting.")]
	public static partial void ApplicationStarting(ILogger logger);

	/// <summary>
	/// Logs that the application is exiting.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	[LoggerMessage(LogEvents.Application.Exiting, LogLevel.Information, "The application is exiting.")]
	public static partial void ApplicationExiting(ILogger logger);

	/// <summary>
	/// Logs that an exit of the application was requested.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	[LoggerMessage(LogEvents.Application.ExitRequested, LogLevel.Information, "An exit of the application was requested.")]
	public static partial void ApplicationExitRequested(ILogger logger);

	/// <summary>
	/// Logs that a restart of the application was requested.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	[LoggerMessage(LogEvents.Application.RestartRequested, LogLevel.Information, "A restart of the application was requested.")]
	public static partial void ApplicationRestartRequested(ILogger logger);

	/// <summary>
	/// Logs an exception nobody handled at the critical level.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="exception">The exception that reached the dispatcher.</param>
	[LoggerMessage(LogEvents.Application.UnhandledException, LogLevel.Critical, "An unhandled exception reached the dispatcher.")]
	public static partial void UnhandledException(ILogger logger, Exception? exception);
}
