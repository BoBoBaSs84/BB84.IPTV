// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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

	/// <summary>
	/// Gets or sets the path of the downloaded file, <see langword="null"/> while the logo is not cached.
	/// </summary>
	public string? LocalPath { get; set; }

	/// <summary>
	/// Gets or sets the entity tag the server sent with the file, used to skip a download that
	/// would bring the same file again.
	/// </summary>
	public string? ETag { get; set; }

	/// <summary>
	/// Gets or sets the hash of the downloaded file, used to detect a changed logo when the server
	/// sends no entity tag.
	/// </summary>
	public string? ContentHash { get; set; }

	/// <summary>
	/// Gets or sets the size of the downloaded file in bytes.
	/// </summary>
	public long? FileSize { get; set; }

	/// <summary>
	/// Gets or sets the moment the logo was downloaded.
	/// </summary>
	public DateTime? DownloadedAt { get; set; }
}
