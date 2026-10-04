// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents the filter of a search across the logos the catalog knows.
/// </summary>
/// <remarks>
/// The result is paged, see <see cref="Parameters"/>, and ordered by <see cref="SortBy"/>, so the
/// order covers the whole result and not only the page that is read.
/// </remarks>
public sealed class LogoSearchRequest : Parameters
{
	/// <summary>
	/// Gets or initializes the text a logo must hold, in the channel identifier, the channel name,
	/// the feed, the format, the URL or the path of the downloaded file.
	/// </summary>
	/// <remarks>
	/// A text of the form <c>channel@feed</c> is split, so the <c>tvg-id</c> of an entry finds the
	/// logos of its channel.
	/// </remarks>
	public string? SearchText { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org channel the search is limited to, <see langword="null"/> for
	/// every channel.
	/// </summary>
	public string? Channel { get; init; }

	/// <summary>
	/// Gets or initializes which logos the search covers, by what the cache holds for them.
	/// </summary>
	public LogoCacheFilter CacheState { get; init; }

	/// <summary>
	/// Gets or initializes the column the logos are ordered by.
	/// </summary>
	public LogoSortColumn SortBy { get; init; }

	/// <summary>
	/// Gets or initializes whether the logos are ordered the other way round.
	/// </summary>
	public bool Descending { get; init; }
}
