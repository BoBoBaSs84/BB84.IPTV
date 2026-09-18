// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a category in the IPTV system.
/// </summary>
/// <param name="Id">Category ID</param>
/// <param name="Name">Name of the category</param>
/// <param name="Description">Short description of the category</param>
public record CategoryRequest(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("description")] string Description
	);
