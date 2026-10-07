// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

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
	/// Converts a <see cref="GuideResponse"/> to a <see cref="GuideEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static GuideEntity ToEntity(this GuideResponse response) => new()
	{
		Channel = response.Channel,
		Feed = response.Feed,
		Site = response.Site,
		SiteId = response.SiteId,
		SiteName = response.SiteName,
		Lang = response.Lang
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

	/// <summary>
	/// Gets the key that identifies the guide of the import.
	/// </summary>
	/// <param name="response">The imported guide.</param>
	/// <returns>The key of the guide.</returns>
	internal static string GetKey(this GuideResponse response)
		=> CatalogKey.Of(response.Channel, response.Feed, response.Site, response.SiteId, response.Lang);

	/// <summary>
	/// Gets the key that identifies the stored guide.
	/// </summary>
	/// <param name="entity">The stored guide.</param>
	/// <returns>The key of the guide.</returns>
	internal static string GetKey(this GuideEntity entity)
		=> CatalogKey.Of(entity.Channel, entity.Feed, entity.Site, entity.SiteId, entity.Lang);

	/// <summary>
	/// Takes what the import holds into the stored guide.
	/// </summary>
	/// <remarks>
	/// Everything but the name the site uses is part of the key, so only that can change.
	/// </remarks>
	/// <param name="entity">The stored guide.</param>
	/// <param name="response">The imported guide.</param>
	/// <returns><see langword="true"/> if the guide changed.</returns>
	internal static bool Apply(this GuideEntity entity, GuideResponse response)
	{
		if (!Differs(entity.SiteName, response.SiteName))
			return false;

		entity.SiteName = response.SiteName;

		return true;
	}
}
