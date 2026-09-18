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
