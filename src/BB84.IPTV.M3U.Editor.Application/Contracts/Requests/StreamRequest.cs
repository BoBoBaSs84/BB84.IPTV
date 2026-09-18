// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a stream for a channel or feed.
/// </summary>
/// <param name="Channel">Channel ID</param>
/// <param name="Feed">Feed ID</param>
/// <param name="Title">Stream title</param>
/// <param name="Url">Stream URL</param>
/// <param name="Referrer">The Referer request header for the stream</param>
/// <param name="UserAgent">The User-Agent request header for the stream</param>
/// <param name="Quality">Maximum stream quality</param>
public record StreamRequest(
	[property: JsonPropertyName("channel")] string? Channel,
	[property: JsonPropertyName("feed")] string? Feed,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("url")] string Url,
	[property: JsonPropertyName("referrer")] string? Referrer,
	[property: JsonPropertyName("user_agent")] string? UserAgent,
	[property: JsonPropertyName("quality")] string? Quality
	);
