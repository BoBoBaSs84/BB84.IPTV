// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

/// <summary>
/// Represents a contract for a media track entry in an M3U playlist.
/// </summary>
public interface IEntry
{
	/// <summary>
	/// Gets or sets the duration of the track in seconds, where -1 indicates
	/// that the duration is unknown or infinite.
	/// </summary>
	int Duration { get; set; }

	/// <summary>
	/// Gets or sets the title of the track / channel is a mandatory and standard.
	/// parameter.
	/// </summary>
	string Title { get; set; }

	/// <summary>
	/// Gets or sets the link to the media source itself, such as the IPTV stream
	/// or the path to the file name if it is a music track.
	/// </summary>
	string FilePath { get; set; }

	/// <summary>
	/// Gets or sets the named grouping of the track, indicating the channel group
	/// is also unofficial.
	/// </summary>
	string? Grouping { get; set; }

	/// <summary>
	/// Gets or sets the directive lines between <c>#EXTINF</c> and the link that are not modelled
	/// by a dedicated property, e.g. <c>#EXTVLCOPT:http-user-agent=...</c>, one per line.
	/// </summary>
	string? Directives { get; set; }

	/// <summary>
	/// Gets or sets the iptv-org channel identifier the entry was created from, <see langword="null"/>
	/// for an entry that is not based on the catalog.
	/// </summary>
	string? Channel { get; set; }

	/// <summary>
	/// Gets or sets the iptv-org feed identifier the entry was created from, <see langword="null"/>
	/// if the entry belongs to the main feed or is not based on the catalog.
	/// </summary>
	string? Feed { get; set; }

	/// <summary>
	/// Gets or sets the additional metadata associated with the track.
	/// </summary>
	IMetadata Metadata { get; }
}
