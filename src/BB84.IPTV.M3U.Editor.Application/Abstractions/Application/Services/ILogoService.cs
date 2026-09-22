// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that keeps the channel logos on disk, so they are shown and exported
/// without asking the network again.
/// </summary>
public interface ILogoService
{
	/// <summary>
	/// Gets what the logo cache holds.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The number of known, cached and missing logos with the size on disk.</returns>
	Task<LogoCacheStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Downloads the logos that are not cached yet, one per channel.
	/// </summary>
	/// <remarks>
	/// The run publishes its progress, can be cancelled and picks up where it stopped, because a
	/// logo that is already on disk is skipped.
	/// </remarks>
	/// <param name="request">What to download, everything that is missing if omitted.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The number of downloaded logos.</returns>
	Task<int> CacheLogosAsync(LogoCacheRequest? request = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the cached file of a channel, the best logo the catalog knows for it.
	/// </summary>
	/// <param name="channel">The iptv-org channel identifier.</param>
	/// <param name="feed">The iptv-org feed identifier, if the entry belongs to one.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The full path of the cached file, or <see langword="null"/> if nothing is cached.</returns>
	Task<string?> GetLocalPathAsync(string channel, string? feed = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the cached files by the logo URL they were downloaded from.
	/// </summary>
	/// <remarks>
	/// Lets a view or an export turn the <c>tvg-logo</c> of an entry into a local path without a
	/// query per entry.
	/// </remarks>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The full path of the cached file per logo URL.</returns>
	Task<IReadOnlyDictionary<string, string>> GetPathsByUrlAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes every cached file and forgets where they were.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The number of deleted files.</returns>
	Task<int> ClearAsync(CancellationToken cancellationToken = default);
}