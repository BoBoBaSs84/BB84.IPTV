// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a geographical region.
/// </summary>
/// <param name="Code">Code of the region</param>
/// <param name="Name">Full name of the region</param>
/// <param name="Countries">List of countries in the region</param>
public record RegionRequest(
	[property: JsonPropertyName("code")] string Code,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("countries")] IReadOnlyList<string> Countries
	);
