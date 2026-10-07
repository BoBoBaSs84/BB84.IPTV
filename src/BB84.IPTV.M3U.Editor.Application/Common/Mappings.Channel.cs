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
	/// Gets the projection of a <see cref="ChannelEntity"/> to the <see cref="ChannelInfo"/> the site browser recognises a row by.
	/// </summary>
	internal static Expression<Func<ChannelEntity, ChannelInfo>> ChannelToInfo { get; }
		= channel => new ChannelInfo(channel.Channel, channel.Name, channel.Country);

	/// <summary>
	/// Gets the projection of a <see cref="ChannelEntity"/> to its iptv-org identifier, for a search
	/// that only needs to know which channels it found.
	/// </summary>
	internal static Expression<Func<ChannelEntity, string>> ChannelToIdentifier { get; }
		= channel => channel.Channel;

	/// <summary>
	/// Converts a <see cref="ChannelResponse"/> to a <see cref="ChannelEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static ChannelEntity ToEntity(this ChannelResponse response) => new()
	{
		AltNames = [.. response.AltNames],
		Categories = [.. response.Categories],
		Channel = response.Id,
		Closed = response.Closed,
		Country = response.Country,
		IsNsfw = response.IsNsfw,
		Launched = response.Launched,
		Name = response.Name,
		Network = response.Network,
		Owners = [.. response.Owners],
		ReplacedBy = response.ReplacedBy,
		Website = response.Website
	};

	/// <summary>
	/// Creates the catalog response of a <paramref name="channel"/>, with the stream and the logo
	/// that were picked for it.
	/// </summary>
	/// <param name="channel">The channel to map.</param>
	/// <param name="stream">The stream picked for the channel, if it has one.</param>
	/// <param name="logo">The logo picked for the channel, if it has one.</param>
	/// <param name="languages">The languages of the feeds of the channel.</param>
	/// <returns>The catalog response.</returns>
	internal static CatalogChannelResponse ToResponse(this ChannelEntity channel, StreamEntity? stream, LogoEntity? logo, IEnumerable<string> languages) => new()
	{
		Channel = channel.Channel,
		Feed = stream?.Feed,
		Name = channel.Name,
		Country = channel.Country,
		Languages = [.. languages],
		// An empty collection is stored as null, the value converter is not used for null.
		Categories = [.. channel.Categories ?? []],
		IsNsfw = channel.IsNsfw,
		StreamUrl = stream?.Url,
		Quality = stream?.Quality,
		LogoUrl = logo?.Url
	};

	/// <summary>
	/// Gets the key that identifies the channel of the import.
	/// </summary>
	/// <param name="response">The imported channel.</param>
	/// <returns>The key of the channel.</returns>
	internal static string GetKey(this ChannelResponse response)
		=> CatalogKey.Of(response.Id);

	/// <summary>
	/// Gets the key that identifies the stored channel.
	/// </summary>
	/// <param name="entity">The stored channel.</param>
	/// <returns>The key of the channel.</returns>
	internal static string GetKey(this ChannelEntity entity)
		=> CatalogKey.Of(entity.Channel);

	/// <summary>
	/// Takes what the import holds into the stored channel.
	/// </summary>
	/// <param name="entity">The stored channel.</param>
	/// <param name="response">The imported channel.</param>
	/// <returns><see langword="true"/> if the channel changed.</returns>
	internal static bool Apply(this ChannelEntity entity, ChannelResponse response)
	{
		bool changed = false;

		if (Differs(entity.Name, response.Name))
		{
			entity.Name = response.Name;
			changed = true;
		}

		if (Differs(entity.Country, response.Country))
		{
			entity.Country = response.Country;
			changed = true;
		}

		if (entity.IsNsfw != response.IsNsfw)
		{
			entity.IsNsfw = response.IsNsfw;
			changed = true;
		}

		if (entity.Launched != response.Launched)
		{
			entity.Launched = response.Launched;
			changed = true;
		}

		if (entity.Closed != response.Closed)
		{
			entity.Closed = response.Closed;
			changed = true;
		}

		if (Differs(entity.Network, response.Network))
		{
			entity.Network = response.Network;
			changed = true;
		}

		if (Differs(entity.ReplacedBy, response.ReplacedBy))
		{
			entity.ReplacedBy = response.ReplacedBy;
			changed = true;
		}

		if (Differs(entity.Website, response.Website))
		{
			entity.Website = response.Website;
			changed = true;
		}

		if (Differs(entity.AltNames, response.AltNames))
		{
			entity.AltNames = [.. response.AltNames];
			changed = true;
		}

		if (Differs(entity.Categories, response.Categories))
		{
			entity.Categories = [.. response.Categories];
			changed = true;
		}

		if (Differs(entity.Owners, response.Owners))
		{
			entity.Owners = [.. response.Owners];
			changed = true;
		}

		return changed;
	}
}
