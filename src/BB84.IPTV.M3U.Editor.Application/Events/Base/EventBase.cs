// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Events.Base;

/// <summary>
/// Represents the base class for all events in the system.
/// </summary>
public abstract class EventBase : IEvent
{
	/// <inheritdoc/>
	public Guid Id { get; } = Guid.NewGuid();

	/// <inheritdoc/>
	public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
