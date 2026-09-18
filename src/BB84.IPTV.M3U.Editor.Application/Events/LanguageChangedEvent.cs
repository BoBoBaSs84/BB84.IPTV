using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is raised when the application's language has been changed.
/// </summary>
/// <param name="language">The new language that has been set.</param>
public sealed class LanguageChangedEvent(string language) : EventBase
{
	/// <summary>
	/// Gets the new language that has been set.
	/// </summary>
	public string Language { get; } = language;
}
