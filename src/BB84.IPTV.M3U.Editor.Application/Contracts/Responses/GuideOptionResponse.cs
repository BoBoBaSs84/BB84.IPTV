// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents one guide of the iptv-org catalog, as it can be picked for an entry.
/// </summary>
/// <remarks>
/// The same shape serves the guides of a channel and the channels of a site, so one list template
/// fits both.
/// </remarks>
public sealed class GuideOptionResponse
{
	/// <summary>
	/// Gets or initializes the iptv-org channel the guide belongs to.
	/// </summary>
	public string? Channel { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org feed the guide belongs to, not set for a guide that covers
	/// the channel as a whole.
	/// </summary>
	public string? Feed { get; init; }

	/// <summary>
	/// Gets or initializes the site the guide is grabbed from, e.g. <c>example.com</c>.
	/// </summary>
	public required string Site { get; init; }

	/// <summary>
	/// Gets or initializes the identifier the channel has on the site.
	/// </summary>
	public required string SiteId { get; init; }

	/// <summary>
	/// Gets or initializes the name the channel has on the site.
	/// </summary>
	public required string SiteName { get; init; }

	/// <summary>
	/// Gets or initializes the language of the guide, as an ISO 639-1 code.
	/// </summary>
	public required string Lang { get; init; }

	/// <summary>
	/// Gets or initializes the name the channel has in the catalog, <see langword="null"/> if the
	/// catalog does not know the channel.
	/// </summary>
	public string? ChannelName { get; init; }

	/// <summary>
	/// Gets or initializes the country the channel is based in (ISO 3166-1 alpha-2), <see langword="null"/>
	/// if the catalog does not know the channel.
	/// </summary>
	public string? Country { get; init; }
}
