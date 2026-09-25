// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is triggered when the status changes and should be cleared after a delay.
/// </summary>
/// <param name="text">The text associated with the status change.</param>
/// <param name="duration">The duration in milliseconds before the status is cleared.</param>
[GenerateToString]
public sealed partial class DelayedStatusChangedEvent(string text, int duration = 2000) : StatusChangedEvent(text, true, duration)
{ }
