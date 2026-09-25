// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is raised when a restart of the application is requested,
/// typically after a language change or other significant setting change that requires
/// a restart to take effect.
/// </summary>
public sealed class RestartRequestedEvent : EventBase
{ }
