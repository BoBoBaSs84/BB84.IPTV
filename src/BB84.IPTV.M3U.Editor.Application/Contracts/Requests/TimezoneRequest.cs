// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a timezone.
/// </summary>
/// <param name="Id">Timezone ID from tz database</param>
/// <param name="UtcOffset">UTC offset for this time zone</param>
/// <param name="Countries">List of countries included in this time zone</param>
public record TimezoneRequest(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("utc_offset")] string UtcOffset,
	[property: JsonPropertyName("countries")] IReadOnlyList<string> Countries
	);
