// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents a stored playlist in a list of playlists, without its entries.
/// </summary>
public sealed class PlaylistSummaryResponse
{
	/// <summary>
	/// Gets or initializes the identifier of the playlist.
	/// </summary>
	public int Id { get; init; }

	/// <summary>
	/// Gets or initializes the name of the playlist.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Gets or initializes the number of entries in the playlist.
	/// </summary>
	public int EntryCount { get; init; }
}