// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents what one entry of a playlist becomes in a <c>channels.xml</c> for the iptv-org EPG
/// grabber: which site is asked, under which identifier, in which language.
/// </summary>
/// <remarks>
/// The mapping belongs to an entry, but not by a foreign key: saving a playlist replaces all its
/// entries, so the mapping is found by <see cref="EntryKey"/>, the <c>tvg-id</c> of the entry or,
/// if it has none, its URL.
/// </remarks>
public sealed class GuideMappingEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the identifier of the playlist the mapping belongs to.
	/// </summary>
	public int PlaylistId { get; set; }

	/// <summary>
	/// Gets or sets the playlist the mapping belongs to.
	/// </summary>
	public PlaylistEntity? Playlist { get; set; }

	/// <summary>
	/// Gets or sets what identifies the entry the mapping belongs to: its <c>tvg-id</c>, or its URL
	/// if it has no <c>tvg-id</c>.
	/// </summary>
	public required string EntryKey { get; set; }

	/// <summary>
	/// Gets or sets the site the guide is grabbed from, e.g. <c>example.com</c>.
	/// </summary>
	public string? Site { get; set; }

	/// <summary>
	/// Gets or sets the identifier the channel has on the site.
	/// </summary>
	public string? SiteId { get; set; }

	/// <summary>
	/// Gets or sets the language of the guide, e.g. <c>de</c>.
	/// </summary>
	public string? Lang { get; set; }

	/// <summary>
	/// Gets or sets the identifier the guide is written under, the <c>tvg-id</c> of the entry.
	/// </summary>
	public string? XmltvId { get; set; }

	/// <summary>
	/// Gets or sets the name shown in the <c>channels.xml</c>, the title of the entry.
	/// </summary>
	public string? DisplayName { get; set; }
}