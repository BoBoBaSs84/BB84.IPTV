// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a blocklist entry.
/// </summary>
/// <param name="Channel">Channel ID</param>
/// <param name="Reason">Reason for blocking (dmca or nsfw)</param>
/// <param name="Ref">Link to removal request or DMCA takedown notice</param>
public record BlocklistRequest(
	[property: JsonPropertyName("channel")] string Channel,
	[property: JsonPropertyName("reason")] string Reason,
	[property: JsonPropertyName("ref")] string Ref
	);
