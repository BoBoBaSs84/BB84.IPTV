// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

/// <summary>
/// Represents an contract for a M3U playlist, which can contain multiple
/// entries with associated metadata.
/// </summary>
public interface IPlaylist
{
	/// <summary>
	/// Gets or sets the URL for the TV guide.
	/// </summary>
	string? UrlTvg { get; set; }

	/// <summary>
	/// Gets or sets the cache period in milliseconds.
	/// </summary>
	int Cache { get; set; }

	/// <summary>
	/// Gets or sets the deinterlace method to be applied.
	/// </summary>
	Deinterlace Deinterlace { get; set; }

	/// <summary>
	/// Gets or sets the refresh period in seconds, after which the playlist
	/// should be reloaded.
	/// </summary>
	int Refresh { get; set; }

	/// <summary>
	/// Gets or sets the <c>#EXTM3U</c> attributes that are not modelled by a dedicated property,
	/// e.g. <c>x-tvg-url="..."</c>, kept as written so they survive a round trip.
	/// </summary>
	string? AdditionalAttributes { get; set; }

	/// <summary>
	/// Gets the list of entries in the playlist.
	/// </summary>
	IEnumerable<EntryModel> Entries { get; }
}
