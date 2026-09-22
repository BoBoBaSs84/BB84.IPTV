// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents what the logo cache holds.
/// </summary>
public sealed class LogoCacheStatusResponse
{
	/// <summary>
	/// Gets or initializes the number of logos the catalog knows.
	/// </summary>
	public int TotalCount { get; init; }

	/// <summary>
	/// Gets or initializes the number of logos that are downloaded and still on disk.
	/// </summary>
	public int CachedCount { get; init; }

	/// <summary>
	/// Gets or initializes the number of logos that are not cached yet.
	/// </summary>
	public int MissingCount
		=> TotalCount - CachedCount;

	/// <summary>
	/// Gets or initializes the size of the cached files in bytes.
	/// </summary>
	public long CachedBytes { get; init; }
}