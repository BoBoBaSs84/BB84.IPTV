// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents what a synchronization of the whole catalog did.
/// </summary>
/// <remarks>
/// A first run on an empty database reports every record as added.
/// </remarks>
public sealed class CatalogSyncResponse
{
	/// <summary>
	/// Gets or initializes what happened to each list of the catalog, in the order they were read.
	/// </summary>
	public required IReadOnlyList<CatalogKindResponse> Kinds { get; init; }

	/// <summary>
	/// Gets the number of rows that were added.
	/// </summary>
	public int TotalAdded => Kinds.Sum(kind => kind.Added);

	/// <summary>
	/// Gets the number of rows that were updated.
	/// </summary>
	public int TotalUpdated => Kinds.Sum(kind => kind.Updated);

	/// <summary>
	/// Gets the number of rows that were removed.
	/// </summary>
	public int TotalRemoved => Kinds.Sum(kind => kind.Removed);

	/// <summary>
	/// Gets the number of lists that were left alone, because they came back without a record.
	/// </summary>
	public int SkippedCount => Kinds.Count(kind => kind.Outcome is CatalogSyncOutcome.Skipped);

	/// <summary>
	/// Indicates whether the catalog changed.
	/// </summary>
	public bool HasChanges => TotalAdded + TotalUpdated + TotalRemoved > 0;

	/// <summary>
	/// Indicates whether every list of the catalog was read.
	/// </summary>
	public bool IsSuccess => SkippedCount is 0;
}
