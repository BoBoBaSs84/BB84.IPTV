// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents what is known about the last synchronization of one list of the iptv-org catalog.
/// </summary>
/// <remarks>
/// There is one row per <see cref="CatalogKind"/>, written when the list was read successfully. The
/// times are UTC.
/// </remarks>
public sealed class CatalogSyncEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the list of the catalog the row belongs to.
	/// </summary>
	public required CatalogKind Kind { get; set; }

	/// <summary>
	/// Gets or sets when the list was read for the first time.
	/// </summary>
	public DateTime FirstImported { get; set; }

	/// <summary>
	/// Gets or sets when the list was read the last time, whether it changed or not.
	/// </summary>
	public DateTime LastChecked { get; set; }

	/// <summary>
	/// Gets or sets when the list brought a change the last time.
	/// </summary>
	public DateTime LastChanged { get; set; }

	/// <summary>
	/// Gets or sets how many rows the last run added.
	/// </summary>
	public int Added { get; set; }

	/// <summary>
	/// Gets or sets how many rows the last run updated.
	/// </summary>
	public int Updated { get; set; }

	/// <summary>
	/// Gets or sets how many rows the last run removed.
	/// </summary>
	public int Removed { get; set; }
}
