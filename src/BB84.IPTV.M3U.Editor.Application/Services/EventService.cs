// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents a simple event service for publishing and subscribing to events.
/// </summary>
/// <param name="logger">The logger for logging event-related activities.</param>
internal sealed class EventService(ILogger<EventService> logger) : IEventService
{
	private readonly Dictionary<Type, List<Action<object>>> _subscribers = [];

	public void Subscribe<T>(Action<T> handler) where T : notnull, IEvent
	{
		if (!_subscribers.TryGetValue(typeof(T), out var handlers))
		{
			handlers = [];
			_subscribers[typeof(T)] = handlers;
		}

		handlers.Add(obj => handler((T)obj));
	}

	public void Publish<T>(T message) where T : notnull, IEvent
	{
		Log.EventPublished(logger, typeof(T).Name, message.Id, message.OccurredAt);

		if (_subscribers.TryGetValue(typeof(T), out List<Action<object>>? handlers))
			handlers.ForEach(handler => handler(message));
	}
}
