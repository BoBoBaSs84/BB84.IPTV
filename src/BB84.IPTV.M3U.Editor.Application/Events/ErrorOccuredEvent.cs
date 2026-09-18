using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is triggered when an error occurs in the application.
/// </summary>
/// <param name="message">The error message of the event.</param>
/// <param name="exception">The exception associated with the error event, if any.</param>
public sealed class ErrorOccuredEvent(string message, Exception? exception = null) : EventBase
{
	/// <summary>
	/// Gets the error message of the event.
	/// </summary>
	public string Message { get; } = message;

	/// <summary>
	/// Gets the exception associated with the error event, if any.
	/// </summary>
	public Exception? Exception { get; } = exception;
}
