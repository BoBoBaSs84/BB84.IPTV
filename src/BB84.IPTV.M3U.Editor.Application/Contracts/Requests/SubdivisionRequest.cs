// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a geographical or administrative subdivision within a country.
/// </summary>
/// <param name="Country">ISO 3166-1 alpha-2 code of the country</param>
/// <param name="Name">Subdivision name</param>
/// <param name="Code">ISO 3166-2 code of the subdivision</param>
/// <param name="Parent">ISO 3166-2 code of the parent subdivision</param>
public record SubdivisionRequest(
	[property: JsonPropertyName("country")] string Country,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("code")] string Code,
	[property: JsonPropertyName("parent")] string? Parent
	);
