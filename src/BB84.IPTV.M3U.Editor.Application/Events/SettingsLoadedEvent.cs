using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is triggered when the application settings have been loaded, allowing other
/// components of the application to respond to changes in settings and update their behavior accordingly.
/// </summary>
public sealed class SettingsLoadedEvent : EventBase
{ }
