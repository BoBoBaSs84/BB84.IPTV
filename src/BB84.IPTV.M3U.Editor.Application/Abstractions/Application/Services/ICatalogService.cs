// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that searches the imported iptv-org channel catalog.
/// </summary>
public interface ICatalogService
{
	/// <summary>
	/// Gets the countries, languages and categories a search can be filtered by.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The filter values, empty lists if the catalog has not been imported yet.</returns>
	Task<CatalogFilterResponse> GetFiltersAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Searches the catalog for the channels matching the <paramref name="request"/>.
	/// </summary>
	/// <param name="request">The filter of the search.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The matching channels with their stream and logo, ordered by name.</returns>
	Task<IReadOnlyList<CatalogChannelResponse>> SearchAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);
}
