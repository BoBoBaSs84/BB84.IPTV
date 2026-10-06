// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository abstraction for managing <see cref="GuideEntity"/> instances.
/// </summary>
public interface IGuideRepository : IRepositoryBase<GuideEntity>
{
	/// <summary>
	/// Searches the guides of every site and returns one page of them.
	/// </summary>
	/// <remarks>
	/// The channel name and the country of a row come from the catalog channel the guide names, which
	/// is joined by its identifier because a guide holds no foreign key. The page is cut from one
	/// fixed order, by channel, site and site id, so a guide never moves between pages. A guide
	/// whose channel the catalog does not know is kept, with neither a name nor a country.
	/// </remarks>
	/// <param name="searchText">The text a guide must hold, <see langword="null"/> for every guide.</param>
	/// <param name="site">The site to limit the search to, <see langword="null"/> for every site.</param>
	/// <param name="skip">The number of guides to skip to reach the page.</param>
	/// <param name="take">The number of guides the page holds.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The guides of the page and the number of guides the search found.</returns>
	Task<(IReadOnlyList<GuideOptionResponse> Guides, int TotalCount)> SearchAsync(
		string? searchText,
		string? site,
		int skip,
		int take,
		CancellationToken cancellationToken = default);
}
