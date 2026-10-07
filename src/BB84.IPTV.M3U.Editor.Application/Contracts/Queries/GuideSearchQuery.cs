// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Queries;

/// <summary>
/// Represents the filter of a search across the guides of every site.
/// </summary>
/// <remarks>
/// The result is paged, see <see cref="PagedQuery"/>, in one fixed order, so a row never moves between
/// pages. The grid sorts only the page it shows.
/// </remarks>
public sealed class GuideSearchQuery : PagedQuery
{
	/// <summary>
	/// Gets or initializes the text a guide must hold, in the channel identifier, the channel name,
	/// the site, the site identifier, the site name or the language.
	/// </summary>
	/// <remarks>
	/// A text of the form <c>channel@feed</c> is split, so the identifier a <c>channels.xml</c> holds
	/// finds its guide.
	/// </remarks>
	public string? SearchText { get; init; }

	/// <summary>
	/// Gets or initializes the site the search is limited to, <see langword="null"/> for every site.
	/// </summary>
	public string? Site { get; init; }
}
