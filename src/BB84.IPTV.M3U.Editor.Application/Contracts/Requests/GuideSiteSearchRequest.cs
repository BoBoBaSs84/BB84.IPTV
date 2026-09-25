// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

/// <summary>
/// Represents the filter of a search in the guides of one site.
/// </summary>
/// <remarks>
/// The result is paged, see <see cref="Parameters"/>.
/// </remarks>
public sealed class GuideSiteSearchRequest : Parameters
{
	/// <summary>
	/// Gets or initializes the site whose guides are searched, e.g. <c>example.com</c>.
	/// </summary>
	public required string Site { get; init; }

	/// <summary>
	/// Gets or initializes the text the channel, the site identifier or the site name must contain.
	/// </summary>
	public string? SearchText { get; init; }
}
