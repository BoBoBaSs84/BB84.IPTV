using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is triggered when a warning occurs in the application.
/// </summary>
/// <param name="message">The warning message of the event.</param>
public sealed class WarningOccuredEvent(string message) : EventBase
{
	/// <summary>
	/// Gets the warning message of the event.
	/// </summary>
	public string Message { get; } = message;
}
