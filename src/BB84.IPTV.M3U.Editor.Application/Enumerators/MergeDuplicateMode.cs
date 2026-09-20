// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents what makes two entries of a merge the same entry.
/// </summary>
public enum MergeDuplicateMode
{
	/// <summary>
	/// Every entry is kept, the merge only appends.
	/// </summary>
	None = 0,

	/// <summary>
	/// Entries with the same <c>tvg-id</c> are the same entry.
	/// </summary>
	ByTvgId = 1,

	/// <summary>
	/// Entries with the same URL are the same entry.
	/// </summary>
	ByUrl = 2,

	/// <summary>
	/// Entries with the same <c>tvg-id</c> or the same URL are the same entry.
	/// </summary>
	ByTvgIdOrUrl = 3
}