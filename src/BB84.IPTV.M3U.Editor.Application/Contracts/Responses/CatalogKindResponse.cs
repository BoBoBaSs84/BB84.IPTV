// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents what the synchronization did to one list of the catalog.
/// </summary>
public sealed class CatalogKindResponse
{
	/// <summary>
	/// Gets or initializes the list of the catalog the result belongs to.
	/// </summary>
	public required CatalogKind Kind { get; init; }

	/// <summary>
	/// Gets or initializes what happened to the list.
	/// </summary>
	public required CatalogSyncOutcome Outcome { get; init; }

	/// <summary>
	/// Gets or initializes the number of rows that were added.
	/// </summary>
	public int Added { get; init; }

	/// <summary>
	/// Gets or initializes the number of rows that were updated.
	/// </summary>
	public int Updated { get; init; }

	/// <summary>
	/// Gets or initializes the number of rows that were removed, because they are gone upstream.
	/// </summary>
	public int Removed { get; init; }

	/// <summary>
	/// Gets or initializes the number of rows that were already up to date.
	/// </summary>
	public int Unchanged { get; init; }

	/// <summary>
	/// Gets or initializes the number of records the list held more than once.
	/// </summary>
	public int Duplicates { get; init; }

	/// <summary>
	/// Gets the number of rows the synchronization touched.
	/// </summary>
	public int Changes => Added + Updated + Removed;
}
