// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents an IPTV guide for a specific channel and site.
/// </summary>
/// <param name="Channel">Channel ID</param>
/// <param name="Feed">Feed ID</param>
/// <param name="Site">Site domain name</param>
/// <param name="SiteId">Unique channel ID used on the site</param>
/// <param name="SiteName">	Channel name used on the site</param>
/// <param name="Lang">Language of the guide (ISO 639-1 code)</param>
public record GuideRequest(
	[property: JsonPropertyName("channel")] string? Channel,
	[property: JsonPropertyName("feed")] string? Feed,
	[property: JsonPropertyName("site")] string Site,
	[property: JsonPropertyName("site_id")] string SiteId,
	[property: JsonPropertyName("site_name")] string SiteName,
	[property: JsonPropertyName("lang")] string Lang
	);
