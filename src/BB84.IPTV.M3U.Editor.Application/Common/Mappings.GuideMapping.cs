// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Creates the entity a guide mapping of a playlist is stored as, blank values are stored as
	/// <see langword="null"/>.
	/// </summary>
	/// <param name="mapping">The mapping to convert.</param>
	/// <param name="playlistId">The identifier of the playlist the mapping belongs to.</param>
	/// <returns>The converted entity.</returns>
	internal static GuideMappingEntity ToEntity(this GuideMappingResponse mapping, int playlistId) => new()
	{
		PlaylistId = playlistId,
		EntryKey = mapping.EntryKey,
		Site = mapping.Site.TrimToNull(),
		SiteId = mapping.SiteId.TrimToNull(),
		Lang = mapping.Lang.TrimToNull(),
		XmltvId = mapping.XmltvId.TrimToNull(),
		DisplayName = mapping.DisplayName.TrimToNull()
	};

	/// <summary>
	/// Creates the mapping of a playlist entry from what is stored for it.
	/// </summary>
	/// <param name="entity">The stored mapping.</param>
	/// <param name="entry">The playlist entry the mapping belongs to.</param>
	/// <param name="key">The key that identifies the entry.</param>
	/// <param name="channel">The iptv-org channel of the entry, if it names one.</param>
	/// <returns>The stored mapping of the entry.</returns>
	internal static GuideMappingResponse ToResponse(this GuideMappingEntity entity, EntryModel entry, string key, string? channel) => new()
	{
		EntryKey = key,
		Title = entry.Title,
		Site = entity.Site,
		SiteId = entity.SiteId,
		Lang = entity.Lang,
		XmltvId = entity.XmltvId,
		DisplayName = entity.DisplayName ?? entry.Title,
		Channel = channel,
		Feed = entry.Feed,
		IsStored = true
	};

	/// <summary>
	/// Creates the mapping of a playlist entry that is not stored yet, prefilled from a guide.
	/// </summary>
	/// <param name="entry">The playlist entry to map.</param>
	/// <param name="key">The key that identifies the entry.</param>
	/// <param name="guide">The guide the mapping is prefilled from, if one is known.</param>
	/// <param name="channel">The iptv-org channel of the entry, if it names one.</param>
	/// <returns>The prefilled mapping of the entry.</returns>
	internal static GuideMappingResponse ToGuideMapping(this EntryModel entry, string key, GuideEntity? guide, string? channel) => new()
	{
		EntryKey = key,
		Title = entry.Title,
		Site = guide?.Site,
		SiteId = guide?.SiteId,
		Lang = guide?.Lang,
		XmltvId = entry.Metadata.TvgId,
		DisplayName = entry.Title,
		Channel = channel,
		Feed = entry.Feed,
		IsStored = false
	};
}
