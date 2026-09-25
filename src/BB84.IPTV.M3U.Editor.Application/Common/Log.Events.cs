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
	/// Logs that an event is being published.
	/// </summary>
	/// <param name="logger">The logger that writes the entry.</param>
	/// <param name="eventType">The name of the type of the event.</param>
	/// <param name="identifier">The identifier of the event.</param>
	/// <param name="occurredAt">The point in time the event occurred at.</param>
	[LoggerMessage(LogEvents.Events.Published, LogLevel.Debug,
		"Publishing the event '{EventType}' with the identifier '{Identifier}' at '{OccurredAt}'.")]
	public static partial void EventPublished(ILogger logger, string eventType, Guid identifier, DateTimeOffset occurredAt);
}
