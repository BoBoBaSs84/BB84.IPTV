// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

/// <summary>
/// Maps catalog channels and user defined channels to playlist entries.
/// </summary>
internal static class CatalogExtensions
{
	/// <summary>
	/// Creates a playlist entry from a catalog channel.
	/// </summary>
	/// <param name="channel">The catalog channel to map.</param>
	/// <returns>The new playlist entry.</returns>
	internal static EntryModel ToEntry(this CatalogChannelResponse channel)
	{
		MetadataModel metadata = new()
		{
			TvgId = channel.ToTvgId(),
			TvgName = channel.Name,
			TvgLogo = channel.LogoUrl,
			GroupTitle = channel.Categories.Count > 0 ? channel.Categories[0] : channel.Country,
			Censored = channel.IsNsfw
		};

		return new EntryModel(channel.Name, channel.StreamUrl ?? string.Empty, metadata: metadata)
		{
			Channel = channel.Channel,
			Feed = channel.Feed
		};
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

	/// <summary>
	/// Builds the TV guide identifier (<c>tvg-id</c>) of a catalog channel, as iptv-org writes it:
	/// the channel identifier, followed by the feed identifier for a feed that is not the main one.
	/// </summary>
	/// <param name="channel">The catalog channel.</param>
	/// <returns>The TV guide identifier.</returns>
	internal static string ToTvgId(this CatalogChannelResponse channel)
		=> string.IsNullOrWhiteSpace(channel.Feed) ? channel.Channel : $"{channel.Channel}@{channel.Feed}";
}
