// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents which of two duplicate entries survives a merge.
/// </summary>
public enum MergeDuplicateResolution
{
	/// <summary>
	/// The entry that comes first wins, at the position it has.
	/// </summary>
	KeepFirst = 0,

	/// <summary>
	/// The entry that comes last wins, at the position it has.
	/// </summary>
	KeepLast = 1
}