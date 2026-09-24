// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is raised when saved data path settings differ from the paths in use.
/// </summary>
/// <remarks>
/// The database, the logger and the logo store are set up while the application starts, so the new
/// paths are used after a restart.
/// </remarks>
public sealed class DataPathsChangedEvent : EventBase;