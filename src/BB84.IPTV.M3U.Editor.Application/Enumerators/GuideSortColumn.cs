// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents the column a guide search is ordered by.
/// </summary>
/// <remarks>
/// The names are the property names of
/// <see cref="Contracts.Responses.GuideOptionResponse"/>, so a grid column can be mapped by its
/// sort member path.
/// </remarks>
public enum GuideSortColumn
{
	/// <summary>
	/// Ordered by the iptv-org channel identifier.
	/// </summary>
	Channel = 0,

	/// <summary>
	/// Ordered by the name the catalog knows the channel under.
	/// </summary>
	ChannelName = 1,

	/// <summary>
	/// Ordered by the iptv-org feed identifier.
	/// </summary>
	Feed = 2,

	/// <summary>
	/// Ordered by the site the guide is grabbed from.
	/// </summary>
	Site = 3,

	/// <summary>
	/// Ordered by the identifier the site uses for the channel.
	/// </summary>
	SiteId = 4,

	/// <summary>
	/// Ordered by the name the site uses for the channel.
	/// </summary>
	SiteName = 5,

	/// <summary>
	/// Ordered by the language of the guide.
	/// </summary>
	Lang = 6,

	/// <summary>
	/// Ordered by the country the catalog knows the channel in.
	/// </summary>
	Country = 7
}
