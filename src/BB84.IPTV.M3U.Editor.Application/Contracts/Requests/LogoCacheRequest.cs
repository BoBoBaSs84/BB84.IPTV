// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents what a run of the logo cache downloads.
/// </summary>
public sealed class LogoCacheRequest
{
	/// <summary>
	/// The largest number of logos that may be downloaded at once.
	/// </summary>
	public const int MaxParallelLimit = 16;

	private int _maxParallelDownloads = 4;

	/// <summary>
	/// Gets or initializes a value indicating whether logos that are already cached are downloaded
	/// again, asking the server whether they changed.
	/// </summary>
	public bool RefreshCached { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org channel identifiers to cache, all known channels if empty.
	/// </summary>
	public IReadOnlyList<string> Channels { get; init; } = [];

	/// <summary>
	/// Gets or initializes how many logos are downloaded at once.
	/// </summary>
	/// <remarks>
	/// Kept between one and <see cref="MaxParallelLimit"/>, so a run stays friendly to the hosts
	/// the logos come from. The files are written while they arrive, the database is written
	/// afterwards, one batch at a time.
	/// </remarks>
	public int MaxParallelDownloads
	{
		get => _maxParallelDownloads;
		init => _maxParallelDownloads = Math.Clamp(value, 1, MaxParallelLimit);
	}
}