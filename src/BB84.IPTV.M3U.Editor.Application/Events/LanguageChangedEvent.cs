// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
