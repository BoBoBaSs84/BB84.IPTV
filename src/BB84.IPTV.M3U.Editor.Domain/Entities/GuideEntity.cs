using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a guide entity in the IPTV M3U editor domain, which says where the program guide of
/// a channel or feed is grabbed from.
/// </summary>
public sealed class GuideEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the channel identifier the guide belongs to.
	/// </summary>
	public string? Channel { get; set; }

	/// <summary>
	/// Gets or sets the feed identifier the guide belongs to, which is not set for a guide that
	/// covers the channel as a whole.
	/// </summary>
	public string? Feed { get; set; }

	/// <summary>
	/// Gets or sets the domain name of the site the guide is grabbed from.
	/// </summary>
	public required string Site { get; set; }

	/// <summary>
	/// Gets or sets the identifier the channel has on the site.
	/// </summary>
	public required string SiteId { get; set; }

	/// <summary>
	/// Gets or sets the name the channel has on the site.
	/// </summary>
	public required string SiteName { get; set; }

	/// <summary>
	/// Gets or sets the language of the guide, as an ISO 639-1 code.
	/// </summary>
	public required string Lang { get; set; }
}
