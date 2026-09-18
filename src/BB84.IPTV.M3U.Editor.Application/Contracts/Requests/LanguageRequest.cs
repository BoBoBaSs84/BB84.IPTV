// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.Json.Serialization;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents a language with its name and code.
/// </summary>
/// <param name="Name">Language name</param>
/// <param name="Code">ISO 639-3 code of the language</param>
public record LanguageRequest(
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("code")] string Code
	);
