// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a stored playlist, the <c>#EXTM3U</c> header of an M3U file plus a name.
/// </summary>
public sealed class PlaylistEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the name of the playlist, as shown to the user.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the URL for the TV guide (<c>url-tvg</c>).
	/// </summary>
	public string? UrlTvg { get; set; }

	/// <summary>
	/// Gets or sets the cache period in milliseconds (<c>cache</c>).
	/// </summary>
	public int Cache { get; set; }

	/// <summary>
	/// Gets or sets the deinterlace method (<c>deinterlace</c>).
	/// </summary>
	public Deinterlace Deinterlace { get; set; }

	/// <summary>
	/// Gets or sets the refresh period in seconds (<c>refresh</c>).
	/// </summary>
	public int Refresh { get; set; }

	/// <summary>
	/// Gets or sets the <c>#EXTM3U</c> attributes without a dedicated property, kept as written.
	/// </summary>
	public string? AdditionalAttributes { get; set; }

	/// <summary>
	/// Gets or sets the entries of the playlist, ordered by <see cref="PlaylistEntryEntity.Position"/>.
	/// </summary>
	public ICollection<PlaylistEntryEntity> Entries { get; set; } = [];
}