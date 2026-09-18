using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is published when the application is exiting, allowing subscribers
/// to perform any necessary cleanup or finalization tasks before the application shuts down.
/// </summary>
public sealed class ExitRequestedEvent : EventBase
{ }
