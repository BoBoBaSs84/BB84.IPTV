// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents which logos a search covers, by what the logo cache holds for them.
/// </summary>
public enum LogoCacheFilter
{
	/// <summary>
	/// Every logo the catalog knows, cached or not.
	/// </summary>
	Any = 0,

	/// <summary>
	/// Only the logos that are downloaded.
	/// </summary>
	Cached = 1,

	/// <summary>
	/// Only the logos that are not downloaded yet.
	/// </summary>
	NotCached = 2
}
