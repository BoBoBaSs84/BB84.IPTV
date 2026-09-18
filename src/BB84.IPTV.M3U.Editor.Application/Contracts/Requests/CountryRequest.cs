// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a country with its name, code, languages, and flag.
/// </summary>
/// <param name="Name">Name of the country</param>
/// <param name="Code">ISO 3166-1 alpha-2 code of the country</param>
/// <param name="Languages">List of official languages of the country (ISO 639-3 code)</param>
/// <param name="Flag">Country flag emoji</param>
public record CountryRequest(
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("code")] string Code,
	[property: JsonPropertyName("languages")] IReadOnlyList<string> Languages,
	[property: JsonPropertyName("flag")] string Flag
	);
