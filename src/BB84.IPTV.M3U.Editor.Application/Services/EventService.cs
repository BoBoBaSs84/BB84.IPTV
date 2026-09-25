// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents a simple event service for publishing and subscribing to events.
/// </summary>
/// <param name="loggerService">The logger service for logging event-related activities.</param>
internal sealed class EventService(ILoggerService<EventService> loggerService) : IEventService
{
	private readonly Dictionary<Type, List<Action<object>>> _subscribers = [];
	private static readonly Action<ILogger, string, Exception?> LogDebug =
		LoggerMessage.Define<string>(LogLevel.Debug, 0, "{Debug}");

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
		string debugMessage = $"Publishing event of type '{typeof(T).Name}' with ID '{message.Id}' at '{message.OccurredAt}'.";
		loggerService.Log(LogDebug, debugMessage);
		if (_subscribers.TryGetValue(typeof(T), out List<Action<object>>? handlers))
			handlers.ForEach(handler => handler(message));
	}
}
