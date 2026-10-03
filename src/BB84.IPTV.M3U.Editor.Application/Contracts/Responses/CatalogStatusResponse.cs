// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents what is known about the synchronization of one list of the catalog.
/// </summary>
/// <remarks>
/// A list that was never read has no row, so the screen shows it as never imported. The times are
/// UTC.
/// </remarks>
public sealed class CatalogStatusResponse
{
	/// <summary>
	/// Gets or initializes the list of the catalog the status belongs to.
	/// </summary>
	public required CatalogKind Kind { get; init; }

	/// <summary>
	/// Gets or initializes when the list was read for the first time.
	/// </summary>
	public DateTime? FirstImported { get; init; }

	/// <summary>
	/// Gets or initializes when the list was read the last time.
	/// </summary>
	public DateTime? LastChecked { get; init; }

	/// <summary>
	/// Gets or initializes when the list brought a change the last time.
	/// </summary>
	public DateTime? LastChanged { get; init; }

	/// <summary>
	/// Gets or initializes how many rows the last run added.
	/// </summary>
	public int Added { get; init; }

	/// <summary>
	/// Gets or initializes how many rows the last run updated.
	/// </summary>
	public int Updated { get; init; }

	/// <summary>
	/// Gets or initializes how many rows the last run removed.
	/// </summary>
	public int Removed { get; init; }

	/// <summary>
	/// Indicates whether the list was read at least once.
	/// </summary>
	public bool IsImported => LastChecked.HasValue;
}
