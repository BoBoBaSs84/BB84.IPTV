// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents the moment the logo cache changed, so a view can show the files it now holds.
/// </summary>
/// <param name="cachedCount">The number of logos that were downloaded or deleted.</param>
[GenerateToString]
public sealed partial class LogoCacheChangedEvent(int cachedCount) : EventBase
{
	/// <summary>
	/// Gets the number of logos that were downloaded or deleted.
	/// </summary>
	public int CachedCount => cachedCount;
}