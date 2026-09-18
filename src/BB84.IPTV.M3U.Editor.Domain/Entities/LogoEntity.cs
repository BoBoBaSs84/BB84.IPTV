using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a logo entity with channel information, dimensions, format, tags, and URL.
/// </summary>
public sealed class LogoEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the channel ID associated with the logo.
	/// </summary>
	public required string Channel { get; set; }

	/// <summary>
	/// Gets or sets the feed ID associated with the logo.
	/// </summary>
	public string? Feed { get; set; }

	/// <summary>
	/// Gets or sets the list of keywords describing this version of the logo.
	/// </summary>
	public required ICollection<string> Tags { get; set; }

	/// <summary>
	/// Gets or sets the width of the image in pixels.
	/// </summary>
	public float Width { get; set; }

	/// <summary>
	/// Gets or sets the height of the image in pixels.
	/// </summary>
	public float Height { get; set; }

	/// <summary>
	/// Gets or sets the image format (one of: PNG, JPEG, SVG, GIF, WebP, AVIF, APNG).
	/// </summary>
	public string? Format { get; set; }

	/// <summary>
	/// Gets or sets the logo URL.
	/// </summary>
	public required string Url { get; set; }
}
