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
	/// Logs that a logo could not be downloaded.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="url">The url of the logo.</param>
	/// <param name="exception">The exception the download failed with.</param>
	[LoggerMessage(LogEvents.Logo.DownloadFailed, LogLevel.Warning, "The logo at '{Url}' could not be downloaded.")]
	public static partial void LogoDownloadFailed(ILogger logger, string url, Exception exception);

	/// <summary>
	/// Logs that the host refused a logo.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="url">The url of the logo.</param>
	/// <param name="statusCode">The status code the host answered with.</param>
	[LoggerMessage(LogEvents.Logo.DownloadRefused, LogLevel.Warning, "The host refused the logo at '{Url}' with the status code {StatusCode}.")]
	public static partial void LogoDownloadRefused(ILogger logger, string url, int statusCode);

	/// <summary>
	/// Logs that a logo could not be cached.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="url">The url of the logo.</param>
	/// <param name="exception">The exception the caching failed with.</param>
	[LoggerMessage(LogEvents.Logo.CacheFailed, LogLevel.Warning, "The logo at '{Url}' could not be cached.")]
	public static partial void LogoCacheFailed(ILogger logger, string url, Exception exception);

	/// <summary>
	/// Logs that the logo cache could not be read.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="exception">The exception the cache was read with.</param>
	[LoggerMessage(LogEvents.Logo.CacheRefreshFailed, LogLevel.Error, "The logo cache could not be read.")]
	public static partial void LogoCacheRefreshFailed(ILogger logger, Exception exception);
}
