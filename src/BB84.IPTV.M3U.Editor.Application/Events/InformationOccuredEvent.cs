// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that carries informational messages within the application.
/// </summary>
/// <param name="message">The informational message to be conveyed by the event.</param>
public sealed class InformationOccuredEvent(string message) : EventBase
{
	/// <summary>
	/// Gets the information message of the event.
	/// </summary>
	public string Message { get; } = message;
}
