// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents the result of a merge, before it is stored as a new playlist.
/// </summary>
public sealed class MergePreviewResponse
{
	/// <summary>
	/// Gets or initializes the merged playlist, as it would be stored.
	/// </summary>
	public required IPlaylist Playlist { get; init; }

	/// <summary>
	/// Gets or initializes the number of playlists that were merged.
	/// </summary>
	public int SourceCount { get; init; }

	/// <summary>
	/// Gets or initializes the number of entries the sources hold together, before duplicates are dropped.
	/// </summary>
	public int SourceEntryCount { get; init; }

	/// <summary>
	/// Gets or initializes the number of entries that were dropped as duplicates.
	/// </summary>
	public int DuplicateCount { get; init; }

	/// <summary>
	/// Gets or initializes the group titles of the merged playlist, ordered by name, an entry
	/// without a group title is an empty string.
	/// </summary>
	public IReadOnlyList<string> Groups { get; init; } = [];
}