// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a TV channel with various attributes.
/// </summary>
/// <param name="Id">Unique channel ID</param>
/// <param name="Name">Full name of the channel</param>
/// <param name="AltNames">List of alternative channel names</param>
/// <param name="Network">Name of the network operating the channel</param>
/// <param name="Owners">List of channel owners</param>
/// <param name="Country">Country code from which the broadcast is transmitted (ISO 3166-1 alpha-2)</param>
/// <param name="Categories">List of categories to which this channel belongs</param>
/// <param name="IsNsfw">Indicates whether the channel broadcasts adult content</param>
/// <param name="Launched">Launch date of the channel (YYYY-MM-DD)</param>
/// <param name="Closed">Date on which the channel closed (YYYY-MM-DD)</param>
/// <param name="ReplacedBy">The ID of the channel that this channel was replaced by</param>
/// <param name="Website">Official website URL</param>
public record ChannelRequest(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("alt_names")] IReadOnlyList<string> AltNames,
	[property: JsonPropertyName("network")] string? Network,
	[property: JsonPropertyName("owners")] IReadOnlyList<string> Owners,
	[property: JsonPropertyName("country")] string Country,
	[property: JsonPropertyName("categories")] IReadOnlyList<string> Categories,
	[property: JsonPropertyName("is_nsfw")] bool IsNsfw,
	[property: JsonPropertyName("launched")] DateTime? Launched,
	[property: JsonPropertyName("closed")] DateTime? Closed,
	[property: JsonPropertyName("replaced_by")] string? ReplacedBy,
	[property: JsonPropertyName("website")] string? Website
	);
