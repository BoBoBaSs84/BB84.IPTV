// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Gets the projection of a <see cref="GuideEntity"/> to the two columns the site list is built from.
	/// </summary>
	internal static Expression<Func<GuideEntity, SiteChannel>> GuideToSiteChannel { get; }
		= guide => new SiteChannel(guide.Site, guide.Channel);

	/// <summary>
	/// Converts a <see cref="GuideRequest"/> to a <see cref="GuideEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static GuideEntity ToEntity(this GuideRequest request) => new()
	{
		Channel = request.Channel,
		Feed = request.Feed,
		Site = request.Site,
		SiteId = request.SiteId,
		SiteName = request.SiteName,
		Lang = request.Lang
	};

	/// <summary>
	/// Creates the option a guide is picked by, with what the catalog knows about its channel.
	/// </summary>
	/// <param name="guide">The guide to map.</param>
	/// <param name="channel">What the catalog knows about the channel of the guide, if anything.</param>
	/// <returns>The guide option.</returns>
	internal static GuideOptionResponse ToOption(this GuideEntity guide, ChannelInfo? channel = null) => new()
	{
		Channel = guide.Channel,
		Feed = guide.Feed,
		Site = guide.Site,
		SiteId = guide.SiteId,
		SiteName = guide.SiteName,
		Lang = guide.Lang,
		ChannelName = channel?.Name,
		Country = channel?.Country
	};
}
