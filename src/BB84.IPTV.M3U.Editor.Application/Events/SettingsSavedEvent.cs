// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is raised when the application settings have been successfully saved, allowing
/// other components of the application to respond to changes in settings and update their behavior accordingly.
/// </summary>
public sealed class SettingsSavedEvent : EventBase
{ }
