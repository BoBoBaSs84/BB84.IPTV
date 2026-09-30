// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Gets the projection of a <see cref="CustomChannelEntity"/> to a <see cref="CustomChannelResponse"/>.
	/// </summary>
	internal static Expression<Func<CustomChannelEntity, CustomChannelResponse>> CustomChannelToResponse { get; }
		= channel => new CustomChannelResponse
		{
			Id = channel.Id,
			Name = channel.Name,
			Url = channel.Url,
			GroupTitle = channel.GroupTitle,
			TvgId = channel.TvgId,
			TvgLogo = channel.TvgLogo
		};

	/// <summary>
	/// Creates a new custom channel entity from the <paramref name="channel"/>.
	/// </summary>
	/// <param name="channel">The custom channel to convert.</param>
	/// <returns>The new entity.</returns>
	internal static CustomChannelEntity ToEntity(this CustomChannelResponse channel) => new()
	{
		Name = channel.Name.Trim(),
		Url = channel.Url.Trim(),
		GroupTitle = channel.GroupTitle,
		TvgId = channel.TvgId,
		TvgLogo = channel.TvgLogo
	};

	/// <summary>
	/// Copies the values of the <paramref name="channel"/> to the <paramref name="entity"/>.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="channel">The custom channel to copy from.</param>
	internal static void Apply(this CustomChannelEntity entity, CustomChannelResponse channel)
	{
		entity.Name = channel.Name.Trim();
		entity.Url = channel.Url.Trim();
		entity.GroupTitle = channel.GroupTitle;
		entity.TvgId = channel.TvgId;
		entity.TvgLogo = channel.TvgLogo;
	}

	/// <summary>
	/// Creates a playlist entry from a user defined channel.
	/// </summary>
	/// <param name="channel">The custom channel to map.</param>
	/// <returns>The new playlist entry.</returns>
	internal static EntryModel ToEntry(this CustomChannelResponse channel)
	{
		MetadataModel metadata = new()
		{
			TvgId = channel.TvgId,
			TvgName = channel.Name,
			TvgLogo = channel.TvgLogo,
			GroupTitle = channel.GroupTitle
		};

		return new EntryModel(channel.Name, channel.Url, metadata: metadata);
	}
}
