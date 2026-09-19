// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a stored playlist entry, one <c>#EXTINF</c> block of an M3U file.
/// </summary>
/// <remarks>
/// Catalog data is referenced by the iptv-org identifiers (<see cref="Channel"/>, <see cref="Feed"/>)
/// instead of foreign keys, so entries survive a reset and re-import of the catalog.
/// </remarks>
public sealed class PlaylistEntryEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the identifier of the playlist the entry belongs to.
	/// </summary>
	public int PlaylistId { get; set; }

	/// <summary>
	/// Gets or sets the playlist the entry belongs to.
	/// </summary>
	public PlaylistEntity? Playlist { get; set; }

	/// <summary>
	/// Gets or sets the zero-based position of the entry within the playlist.
	/// </summary>
	public int Position { get; set; }

	/// <summary>
	/// Gets or sets the duration in seconds, -1 if unknown or infinite.
	/// </summary>
	public int Duration { get; set; }

	/// <summary>
	/// Gets or sets the title of the entry.
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Gets or sets the link to the media source, e.g. the stream URL.
	/// </summary>
	public required string Url { get; set; }

	/// <summary>
	/// Gets or sets the grouping (<c>#EXTGRP</c>).
	/// </summary>
	public string? Grouping { get; set; }

	/// <summary>
	/// Gets or sets the directive lines without a dedicated property, e.g. <c>#EXTVLCOPT</c>, one per line.
	/// </summary>
	public string? Directives { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the entry is protected by parental control (<c>censored</c>).
	/// </summary>
	public bool Censored { get; set; }

	/// <summary>
	/// Gets or sets the TV guide identifier (<c>tvg-id</c>).
	/// </summary>
	public string? TvgId { get; set; }

	/// <summary>
	/// Gets or sets the TV guide name (<c>tvg-name</c>).
	/// </summary>
	public string? TvgName { get; set; }

	/// <summary>
	/// Gets or sets the logo URL or file path (<c>tvg-logo</c>).
	/// </summary>
	public string? TvgLogo { get; set; }

	/// <summary>
	/// Gets or sets the group identifier (<c>group_id</c>).
	/// </summary>
	public string? GroupId { get; set; }

	/// <summary>
	/// Gets or sets the group title (<c>group-title</c>).
	/// </summary>
	public string? GroupTitle { get; set; }

	/// <summary>
	/// Gets or sets the <c>#EXTINF</c> attributes without a dedicated property, kept as written.
	/// </summary>
	public string? AdditionalAttributes { get; set; }

	/// <summary>
	/// Gets or sets the iptv-org channel identifier the entry was created from, if any.
	/// </summary>
	public string? Channel { get; set; }

	/// <summary>
	/// Gets or sets the iptv-org feed identifier the entry was created from, if any.
	/// </summary>
	public string? Feed { get; set; }
}