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
}
