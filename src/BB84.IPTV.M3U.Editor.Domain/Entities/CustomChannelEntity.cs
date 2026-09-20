// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a user defined channel, a stream that is not part of the iptv-org catalog.
/// </summary>
/// <remarks>
/// Any URL scheme is allowed, e.g. <c>http</c>, <c>https</c>, <c>rtsp</c> or <c>udp</c>, so streams from
/// the local network can be used as well.
/// </remarks>
public sealed class CustomChannelEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the name of the channel, used as the title of a playlist entry.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the link to the media source, e.g. the stream URL.
	/// </summary>
	public required string Url { get; set; }

	/// <summary>
	/// Gets or sets the group title (<c>group-title</c>) of a playlist entry created from the channel.
	/// </summary>
	public string? GroupTitle { get; set; }

	/// <summary>
	/// Gets or sets the TV guide identifier (<c>tvg-id</c>) of a playlist entry created from the channel.
	/// </summary>
	public string? TvgId { get; set; }

	/// <summary>
	/// Gets or sets the logo URL or file path (<c>tvg-logo</c>) of a playlist entry created from the channel.
	/// </summary>
	public string? TvgLogo { get; set; }
}
