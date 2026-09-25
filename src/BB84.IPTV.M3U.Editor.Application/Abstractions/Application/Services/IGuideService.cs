// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that maps the entries of a playlist to the guide sites of iptv-org and
/// writes the <c>channels.xml</c> the EPG grabber reads.
/// </summary>
public interface IGuideService
{
	/// <summary>
	/// Gets one mapping per entry of the playlist, in the order of the entries.
	/// </summary>
	/// <remarks>
	/// A mapping that was stored before wins; the others are prefilled from the imported guides,
	/// matched by the <c>tvg-id</c> of the entry, and are empty for an entry that no guide knows.
	/// </remarks>
	/// <param name="playlistId">The identifier of the playlist.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The mappings, empty if the playlist does not exist.</returns>
	Task<IReadOnlyList<GuideMappingResponse>> GetMappingsAsync(int playlistId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores the mappings of a playlist, replacing the ones that were stored before.
	/// </summary>
	/// <param name="playlistId">The identifier of the playlist.</param>
	/// <param name="mappings">The mappings to store, an empty one is not stored.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The number of stored mappings.</returns>
	Task<int> SaveMappingsAsync(int playlistId, IEnumerable<GuideMappingResponse> mappings, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes the mappings of a playlist as a <c>channels.xml</c>.
	/// </summary>
	/// <param name="playlistId">The identifier of the playlist.</param>
	/// <param name="filePath">The path of the file to write.</param>
	/// <param name="mappings">The mappings to write, the stored ones if omitted.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The number of written channels.</returns>
	Task<int> ExportAsync(int playlistId, string filePath, IEnumerable<GuideMappingResponse>? mappings = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the guides the catalog knows for a channel, the ones of the feed first.
	/// </summary>
	/// <param name="channel">The iptv-org channel identifier, e.g. <c>DasErste.de</c>.</param>
	/// <param name="feed">The iptv-org feed identifier the entry was created from, if any.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The guides, empty if no guide is known for the channel.</returns>
	Task<IReadOnlyList<GuideOptionResponse>> GetOptionsAsync(string channel, string? feed = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the sites the program guides are grabbed from, with what each of them covers.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The sites, ordered by name, empty if no guides were imported.</returns>
	Task<IReadOnlyList<GuideSiteResponse>> GetSitesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Searches the guides of one site, so the channels and feeds it covers can be looked through.
	/// </summary>
	/// <param name="request">The site to look at and what to search for.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The page of guides the site holds.</returns>
	Task<IPagedList<GuideOptionResponse>> SearchSiteChannelsAsync(GuideSiteSearchRequest request, CancellationToken cancellationToken = default);
}
