using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is raised when the application settings have been successfully saved, allowing
/// other components of the application to respond to changes in settings and update their behavior accordingly.
/// </summary>
public sealed class SettingsSavedEvent : EventBase
{ }
