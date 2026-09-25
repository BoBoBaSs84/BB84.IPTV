// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a TV channel with various attributes.
/// </summary>
public sealed class ChannelEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the channel identifier, which is a unique string used to identify the
	/// channel. This is a required property and must be provided when creating a new instance.
	/// </summary>
	public required string Channel { get; set; }

	/// <summary>
	/// Gets or sets the name of the channel, which is a human-readable string representing
	/// the channel's name. This is a required property and must be provided when creating a
	/// new instance.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the country associated with the channel, which is a string representing the
	/// Country where the channel is based or primarily broadcasts. This is a required property and
	/// must be provided when creating a new instance.
	/// </summary>
	public required string Country { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the channel is marked as Not Safe For Work (NSFW).
	/// </summary>
	public bool IsNsfw { get; set; }

	/// <summary>
	/// Gets or sets the launch date of the channel, which is a nullable DateTime representing when the
	/// Channel was launched. This property is optional and can be null if the launch date is unknown or
	/// not applicable.
	/// </summary>
	public DateTime? Launched { get; set; }

	/// <summary>
	/// Gets or sets the closed date of the channel, which is a nullable DateTime representing when the
	/// Channel was closed or ceased operations. This property is optional and can be null if the channel
	/// is still active or if the closed date is unknown or not applicable.
	/// </summary>
	public DateTime? Closed { get; set; }

	/// <summary>
	/// Gets or sets the network associated with the channel, which is a string representing the network
	/// or broadcasting company that owns or operates the channel. This property is optional and can be
	/// null if the network information is unknown or not applicable.
	/// </summary>
	public string? Network { get; set; }

	/// <summary>
	/// Gets or sets the replacement channel identifier, which is a string representing the identifier of
	/// the channel that replaces this channel if it has been closed or ceased operations. This property is
	/// nullable and can be null if the channel is still active or if there is no replacement channel.
	/// </summary>
	public string? ReplacedBy { get; set; }

	/// <summary>
	/// Gets or sets the website associated with the channel, which is a string representing the URL of the
	/// Channel's official website or online presence. This property is optional and can be null if the website
	/// information is unknown or not applicable.
	/// </summary>
	public string? Website { get; set; }

	/// <summary>
	/// Gets or sets the owners of the channel, which is a collection of strings representing the individuals,
	/// organizations, or entities that own or operate the channel.
	/// </summary>
	public ICollection<string> Owners { get; set; } = [];

	/// <summary>
	/// Gets or sets the alternative names of the channel, which is a collection of strings representing any
	/// alternative names or aliases that the channel may be known by. This can include former names, nicknames,
	/// or any other names that are associated with the channel.
	/// </summary>
	public ICollection<string> AltNames { get; set; } = [];

	/// <summary>
	/// Gets or sets the categories of the channel, which is a collection of strings representing the genres, types,
	/// subjects, or classifications that the channel belongs to.
	/// </summary>
	public ICollection<string> Categories { get; set; } = [];
}
