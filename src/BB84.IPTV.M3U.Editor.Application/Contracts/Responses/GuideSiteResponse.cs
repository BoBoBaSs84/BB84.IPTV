// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents one site the program guides are grabbed from, with what it covers.
/// </summary>
public sealed class GuideSiteResponse
{
	/// <summary>
	/// Gets or initializes the domain name of the site, e.g. <c>example.com</c>.
	/// </summary>
	public required string Site { get; init; }

	/// <summary>
	/// Gets or initializes the number of channels the site holds a guide for.
	/// </summary>
	public int ChannelCount { get; init; }

	/// <summary>
	/// Gets or initializes the number of guides of the site, which is larger than the number of
	/// channels when a channel is covered per feed or in more than one language.
	/// </summary>
	public int GuideCount { get; init; }
}
