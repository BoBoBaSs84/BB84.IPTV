// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a feed entity in the IPTV M3U editor domain.
/// </summary>
public sealed class FeedEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the channel identifier associated with the feed entity.
	/// </summary>
	public required string Channel { get; set; }

	/// <summary>
	/// Gets or sets the feed identifier associated with the feed entity.
	/// </summary>
	public required string Feed { get; set; }

	/// <summary>
	/// Gets or sets the name associated with the feed entity.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this feed entity is the main
	/// feed for the associated channel.
	/// </summary>
	public bool IsMain { get; set; }

	/// <summary>
	/// Gets or sets the format of the feed entity, which may indicate the type
	/// of content or data format used by the feed.
	/// </summary>
	public required string Format { get; set; }

	/// <summary>
	/// Gets or sets the alternative names of the feed, which is a collection of strings representing
	/// any other names the feed is known by.
	/// </summary>
	public ICollection<string> AltNames { get; set; } = [];

	/// <summary>
	/// Gets or sets the broadcasting area of the feed, which is a collection of codes in the form
	/// <c>r/&lt;region_code&gt;</c>, <c>c/&lt;country_code&gt;</c>, <c>s/&lt;subdivision_code&gt;</c>
	/// or <c>ct/&lt;city_code&gt;</c>.
	/// </summary>
	public ICollection<string> BroadcastArea { get; set; } = [];

	/// <summary>
	/// Gets or sets the timezones the feed is broadcast in, which is a collection of timezone
	/// identifiers.
	/// </summary>
	public ICollection<string> Timezones { get; set; } = [];

	/// <summary>
	/// Gets or sets the languages the feed is broadcast in, which is a collection of ISO 639-3
	/// codes.
	/// </summary>
	public ICollection<string> Languages { get; set; } = [];
}
