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

	public ICollection<string> AltNames { get; set; } = [];
	public ICollection<string> BroadcastArea { get; set; } = [];
	public ICollection<string> Timezones { get; set; } = [];
	public ICollection<string> Languages { get; set; } = [];
}
