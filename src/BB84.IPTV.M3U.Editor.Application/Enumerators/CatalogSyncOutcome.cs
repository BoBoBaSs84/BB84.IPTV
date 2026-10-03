// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents what happened to one list of the catalog during a synchronization.
/// </summary>
public enum CatalogSyncOutcome
{
	/// <summary>
	/// The list was read and the catalog was brought in line with it.
	/// </summary>
	Synchronized = 0,

	/// <summary>
	/// The list came back without a single record, so nothing was changed.
	/// </summary>
	/// <remarks>
	/// iptv-org answers a failed request with no records as well, so an empty list is never taken
	/// as "everything was removed".
	/// </remarks>
	Skipped = 1
}
