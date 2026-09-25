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
	/// Logs that a request was sent to the api.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="url">The url the request was sent to.</param>
	[LoggerMessage(LogEvents.Web.ApiRequestSent, LogLevel.Information, "Sending a request to '{Url}'.")]
	public static partial void ApiRequestSent(ILogger logger, string url);

	/// <summary>
	/// Logs how many items the api answered with.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="count">The number of items that were received.</param>
	/// <param name="url">The url the items came from.</param>
	[LoggerMessage(LogEvents.Web.ApiItemsReceived, LogLevel.Information, "Received {Count} items from '{Url}'.")]
	public static partial void ApiItemsReceived(ILogger logger, int count, string url);

	/// <summary>
	/// Logs that the api answered without items.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="url">The url that answered without items.</param>
	[LoggerMessage(LogEvents.Web.ApiNoItemsReceived, LogLevel.Information, "Received no items from '{Url}'.")]
	public static partial void ApiNoItemsReceived(ILogger logger, string url);
}
