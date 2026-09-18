// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a logo for a channel or feed.
/// </summary>
/// <param name="Channel">Channel ID</param>
/// <param name="Feed">Feed ID</param>
/// <param name="Tags">List of keywords describing this version of the logo</param>
/// <param name="Width">The width of the image in pixels</param>
/// <param name="Height">The height of the image in pixels</param>
/// <param name="Format">Image format (one of: PNG, JPEG, SVG, GIF, WebP, AVIF, APNG)</param>
/// <param name="Url">Logo URL</param>
public record LogoRequest(
	[property: JsonPropertyName("channel")] string Channel,
	[property: JsonPropertyName("feed")] string? Feed,
	[property: JsonPropertyName("tags")] IReadOnlyList<string> Tags,
	[property: JsonPropertyName("width")] float Width,
	[property: JsonPropertyName("height")] float Height,
	[property: JsonPropertyName("format")] string? Format,
	[property: JsonPropertyName("url")] string Url
	);
