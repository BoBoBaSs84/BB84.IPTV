// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents the playlists to merge and how they are merged.
/// </summary>
/// <remarks>
/// The sources are never modified, the merge always produces a new playlist.
/// </remarks>
public sealed class MergeRequest
{
	/// <summary>
	/// Gets or initializes the identifiers of the playlists to merge, in the order they are appended.
	/// </summary>
	public required IReadOnlyList<int> PlaylistIds { get; init; }

	/// <summary>
	/// Gets or initializes the name of the new playlist.
	/// </summary>
	public string? Name { get; init; }

	/// <summary>
	/// Gets or initializes what makes two entries the same entry.
	/// </summary>
	public MergeDuplicateMode DuplicateMode { get; init; }

	/// <summary>
	/// Gets or initializes which of two duplicate entries survives.
	/// </summary>
	public MergeDuplicateResolution DuplicateResolution { get; init; }

	/// <summary>
	/// Gets or initializes the group titles (<c>group-title</c>) to rename, the old title as key.
	/// </summary>
	/// <remarks>
	/// An entry without a group title is reached with an empty key, a target that is empty clears
	/// the group title.
	/// </remarks>
	public IReadOnlyDictionary<string, string>? GroupMappings { get; init; }
}