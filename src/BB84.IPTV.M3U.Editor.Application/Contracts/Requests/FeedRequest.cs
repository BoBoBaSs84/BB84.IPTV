// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a feed associated with a TV channel.
/// </summary>
/// <param name="Channel">Channel ID</param>
/// <param name="Id">Unique feed ID</param>
/// <param name="Name">Name of the feed</param>
/// <param name="AltNames">List of alternative feed names</param>
/// <param name="IsMain">Indicates if this feed is the main for the channel</param>
/// <param name="BroadcastArea">List of codes describing the broadcasting area (r/&lt;region_code&gt;, c/&lt;country_code&gt;, s/&lt;subdivision_code&gt;, ct/&lt;city_code&gt;)</param>
/// <param name="Timezones">List of timezones in which the feed is broadcast</param>
/// <param name="Languages">List of broadcast languages</param>
/// <param name="Format">Video format of the feed</param>
public record FeedRequest(
	[property: JsonPropertyName("channel")] string Channel,
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("alt_names")] IReadOnlyList<string> AltNames,
	[property: JsonPropertyName("is_main")] bool IsMain,
	[property: JsonPropertyName("broadcast_area")] IReadOnlyList<string> BroadcastArea,
	[property: JsonPropertyName("timezones")] IReadOnlyList<string> Timezones,
	[property: JsonPropertyName("languages")] IReadOnlyList<string> Languages,
	[property: JsonPropertyName("format")] string Format
	);
