// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Events;

/// <summary>
/// Represents a domain event that has occurred within the system. Domain events are used to capture
/// significant occurrences or changes in the state of the domain model, allowing for decoupled
/// communication between different parts of the system. Each event should contain relevant information
/// about what happened and when it happened, enabling other components to react accordingly.
/// </summary>
public interface IEvent
{
	/// <summary>
	/// Gets the timestamp when the event occurred.
	/// </summary>
	DateTimeOffset OccurredAt { get; }

	/// <summary>
	/// Gets the unique identifier of the event.
	/// </summary>
	Guid Id { get; }
}
