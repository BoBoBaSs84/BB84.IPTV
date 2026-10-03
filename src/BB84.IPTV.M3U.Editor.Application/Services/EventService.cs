// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.Concurrent;
using System.Collections.Immutable;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents a simple event service for publishing and subscribing to events.
/// </summary>
/// <remarks>
/// The handlers are stored as the delegates they were registered with, so a subscriber can be
/// removed again, and the list of one event type is immutable, so a handler that subscribes or
/// unsubscribes while an event is published does not disturb the run.
/// </remarks>
/// <param name="logger">The logger for logging event-related activities.</param>
internal sealed class EventService(ILogger<EventService> logger) : IEventService
{
	private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> _subscribers = new();

	public void Subscribe<T>(Action<T> handler) where T : notnull, IEvent
	{
		ArgumentNullException.ThrowIfNull(handler);

		_ = _subscribers.AddOrUpdate(typeof(T), _ => [handler], (_, handlers) => handlers.Add(handler));
	}

	public void Unsubscribe<T>(Action<T> handler) where T : notnull, IEvent
	{
		ArgumentNullException.ThrowIfNull(handler);

		_ = _subscribers.AddOrUpdate(typeof(T), _ => [], (_, handlers) => handlers.Remove(handler));
	}

	public void Publish<T>(T message) where T : notnull, IEvent
	{
		Log.EventPublished(logger, typeof(T).Name, message.Id, message.OccurredAt);

		if (!_subscribers.TryGetValue(typeof(T), out ImmutableList<Delegate>? handlers))
			return;

		foreach (Delegate handler in handlers)
			((Action<T>)handler).Invoke(message);
	}
}
