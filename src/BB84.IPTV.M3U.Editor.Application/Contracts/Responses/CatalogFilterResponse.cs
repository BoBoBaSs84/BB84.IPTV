// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents the values a catalog search can be filtered by.
/// </summary>
public sealed class CatalogFilterResponse
{
	/// <summary>
	/// Gets or initializes the countries, ordered by name.
	/// </summary>
	public IReadOnlyList<CatalogFilterValue> Countries { get; init; } = [];

	/// <summary>
	/// Gets or initializes the languages, ordered by name.
	/// </summary>
	public IReadOnlyList<CatalogFilterValue> Languages { get; init; } = [];

	/// <summary>
	/// Gets or initializes the categories, ordered by name.
	/// </summary>
	public IReadOnlyList<CatalogFilterValue> Categories { get; init; } = [];
}
