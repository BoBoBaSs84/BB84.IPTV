// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a city within a subdivision and country.
/// </summary>
/// <param name="Country">ISO 3166-1 alpha-2 code of the country where the city is located</param>
/// <param name="Subdivision">ISO 3166-2 code of the subdivision where the city is located</param>
/// <param name="Name">City name</param>
/// <param name="Code">UN/LOCODE of the city</param>
/// <param name="WikidataId">ID of this city in Wikidata</param>
public record CityRequest(
	[property: JsonPropertyName("country")] string Country,
	[property: JsonPropertyName("subdivision")] string? Subdivision,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("code")] string Code,
	[property: JsonPropertyName("wikidata_id")] string WikidataId
	);
