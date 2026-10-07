// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Queries;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;

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
	/// Searches the logos the catalog knows and returns one page of them.
	/// </summary>
	/// <remarks>
	/// Every logo of a channel is returned, not only the one that is picked for it, so a logo of
	/// another feed, format or tag can be looked at and assigned. Whether the file of a row is
	/// really there is asked of the store for the page that is returned.
	/// </remarks>
	/// <param name="query">What to search for, how to order it and which page to read.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The logos of the page and what the search found.</returns>
	Task<IPagedList<LogoOptionResponse>> SearchLogosAsync(LogoSearchQuery query, CancellationToken cancellationToken = default);

	/// <summary>
	/// Downloads one logo, whether or not it is the one that is picked for its channel.
	/// </summary>
	/// <remarks>
	/// Lets a view cache the logo it wants to assign, instead of running the whole cache for it. A
	/// downloaded logo is reported with a logo cache changed event, so the views read the cache
	/// again.
	/// </remarks>
	/// <param name="logoId">The identifier of the stored logo.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The path of the cached file, or <see langword="null"/> if nothing was downloaded.</returns>
	Task<string?> CacheLogoAsync(int logoId, CancellationToken cancellationToken = default);

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