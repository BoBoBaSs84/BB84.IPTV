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
	/// Gets the projection of a <see cref="ChannelEntity"/> to the <see cref="ChannelInfo"/> the site browser recognises a row by.
	/// </summary>
	internal static Expression<Func<ChannelEntity, ChannelInfo>> ChannelToInfo { get; }
		= channel => new ChannelInfo(channel.Channel, channel.Name, channel.Country);

	/// <summary>
	/// Converts a <see cref="ChannelRequest"/> to a <see cref="ChannelEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static ChannelEntity ToEntity(this ChannelRequest request) => new()
	{
		AltNames = [.. request.AltNames],
		Categories = [.. request.Categories],
		Channel = request.Id,
		Closed = request.Closed,
		Country = request.Country,
		IsNsfw = request.IsNsfw,
		Launched = request.Launched,
		Name = request.Name,
		Network = request.Network,
		Owners = [.. request.Owners],
		ReplacedBy = request.ReplacedBy,
		Website = request.Website
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
	/// <param name="request">The imported channel.</param>
	/// <returns>The key of the channel.</returns>
	internal static string GetKey(this ChannelRequest request)
		=> CatalogKey.Of(request.Id);

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
	/// <param name="request">The imported channel.</param>
	/// <returns><see langword="true"/> if the channel changed.</returns>
	internal static bool Apply(this ChannelEntity entity, ChannelRequest request)
	{
		bool changed = false;

		if (Differs(entity.Name, request.Name))
		{
			entity.Name = request.Name;
			changed = true;
		}

		if (Differs(entity.Country, request.Country))
		{
			entity.Country = request.Country;
			changed = true;
		}

		if (entity.IsNsfw != request.IsNsfw)
		{
			entity.IsNsfw = request.IsNsfw;
			changed = true;
		}

		if (entity.Launched != request.Launched)
		{
			entity.Launched = request.Launched;
			changed = true;
		}

		if (entity.Closed != request.Closed)
		{
			entity.Closed = request.Closed;
			changed = true;
		}

		if (Differs(entity.Network, request.Network))
		{
			entity.Network = request.Network;
			changed = true;
		}

		if (Differs(entity.ReplacedBy, request.ReplacedBy))
		{
			entity.ReplacedBy = request.ReplacedBy;
			changed = true;
		}

		if (Differs(entity.Website, request.Website))
		{
			entity.Website = request.Website;
			changed = true;
		}

		if (Differs(entity.AltNames, request.AltNames))
		{
			entity.AltNames = [.. request.AltNames];
			changed = true;
		}

		if (Differs(entity.Categories, request.Categories))
		{
			entity.Categories = [.. request.Categories];
			changed = true;
		}

		if (Differs(entity.Owners, request.Owners))
		{
			entity.Owners = [.. request.Owners];
			changed = true;
		}

		return changed;
	}
}
