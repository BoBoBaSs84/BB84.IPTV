// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents the filter of a search in the iptv-org channel catalog.
/// </summary>
/// <remarks>
/// All filters are combined, a filter that is <see langword="null"/> or empty is not applied.
/// The result is paged, see <see cref="Parameters"/>.
/// </remarks>
public sealed class CatalogSearchRequest : Parameters
{
	/// <summary>
	/// Gets or initializes the text the channel name or the iptv-org identifier must contain.
	/// </summary>
	public string? SearchText { get; init; }

	/// <summary>
	/// Gets or initializes the country code (ISO 3166-1 alpha-2) the channel must be based in.
	/// </summary>
	public string? Country { get; init; }

	/// <summary>
	/// Gets or initializes the language code (ISO 639-3) one of the feeds of the channel must broadcast in.
	/// </summary>
	public string? Language { get; init; }

	/// <summary>
	/// Gets or initializes the category the channel must belong to.
	/// </summary>
	public string? Category { get; init; }

	/// <summary>
	/// Gets or initializes a value indicating whether channels marked as NSFW are part of the result.
	/// </summary>
	public bool IncludeNsfw { get; init; }

	/// <summary>
	/// Gets or initializes a value indicating whether channels without a stream are part of the result.
	/// </summary>
	public bool IncludeWithoutStream { get; init; }
}