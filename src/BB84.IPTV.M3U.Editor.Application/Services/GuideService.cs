// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that maps the entries of a playlist to the guide sites of iptv-org.
/// </summary>
/// <param name="serviceScopeFactory">The scope factory used to resolve the scoped repository service per call.</param>
/// <param name="playlistService">The service that loads the playlists.</param>
/// <param name="channelsXmlSerializer">The serializer that writes the <c>channels.xml</c>.</param>
/// <param name="providerService">The provider service used for file access.</param>
internal sealed class GuideService(
	IServiceScopeFactory serviceScopeFactory,
	IPlaylistService playlistService,
	IChannelsXmlSerializer channelsXmlSerializer,
	IProviderService providerService) : IGuideService
{
	public async Task<IReadOnlyList<GuideMappingResponse>> GetMappingsAsync(int playlistId, CancellationToken cancellationToken = default)
	{
		IPlaylist? playlist = await playlistService
			.LoadAsync(playlistId, cancellationToken)
			.ConfigureAwait(false);

		if (playlist is null)
			return [];

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<GuideMappingEntity> stored = await repositoryService.GuideMappings
			.GetListAsync(new Query<GuideMappingEntity> { Where = mapping => mapping.PlaylistId == playlistId }, cancellationToken)
			.ConfigureAwait(false);

		Dictionary<string, GuideMappingEntity> storedByKey = stored
			.GroupBy(mapping => mapping.EntryKey, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

		ILookup<string, GuideEntity> guidesByChannel = await LoadGuidesAsync(repositoryService, playlist, cancellationToken)
			.ConfigureAwait(false);

		List<GuideMappingResponse> mappings = [];

		foreach (EntryModel entry in playlist.Entries)
		{
			string key = GetEntryKey(entry);

			if (string.IsNullOrWhiteSpace(key))
				continue;

			mappings.Add(storedByKey.TryGetValue(key, out GuideMappingEntity? entity)
				? ToResponse(entity, entry, key)
				: Prefill(entry, key, guidesByChannel));
		}

		return mappings;
	}

	public async Task<int> SaveMappingsAsync(int playlistId, IEnumerable<GuideMappingResponse> mappings, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(mappings);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		// Replacing everything of the playlist in one commit keeps the stored mappings and the
		// ones on screen the same, also when a row was cleared.
		_ = await repositoryService.GuideMappings
			.ExecuteDeleteAsync(mapping => mapping.PlaylistId == playlistId, cancellationToken)
			.ConfigureAwait(false);

		List<GuideMappingEntity> entities = [.. mappings
			.Where(HoldsSomething)
			.Select(mapping => new GuideMappingEntity
			{
				PlaylistId = playlistId,
				EntryKey = mapping.EntryKey,
				Site = Clean(mapping.Site),
				SiteId = Clean(mapping.SiteId),
				Lang = Clean(mapping.Lang),
				XmltvId = Clean(mapping.XmltvId),
				DisplayName = Clean(mapping.DisplayName)
			})];

		if (entities.Count > 0)
		{
			await repositoryService.GuideMappings
				.CreateAsync(entities, cancellationToken)
				.ConfigureAwait(false);
		}

		_ = await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return entities.Count;
	}

	public async Task<int> ExportAsync(int playlistId, string filePath, IEnumerable<GuideMappingResponse>? mappings = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		IReadOnlyList<GuideMappingResponse> toWrite = mappings is null
			? await GetMappingsAsync(playlistId, cancellationToken).ConfigureAwait(false)
			: [.. mappings];

		string content = channelsXmlSerializer.Serialize(toWrite);

		await providerService.File
			.WriteAllTextAsync(filePath, content, cancellationToken)
			.ConfigureAwait(false);

		return toWrite.Count(mapping => mapping.IsComplete);
	}

	/// <summary>
	/// Loads the guides of the channels the playlist knows, by the channel identifier.
	/// </summary>
	private static async Task<ILookup<string, GuideEntity>> LoadGuidesAsync(IRepositoryService repositoryService, IPlaylist playlist, CancellationToken cancellationToken)
	{
		List<string> channels = [.. playlist.Entries
			.Select(GetChannelKey)
			.Where(channel => !string.IsNullOrWhiteSpace(channel))
			.Distinct(StringComparer.OrdinalIgnoreCase)];

		if (channels.Count is 0)
			return Array.Empty<GuideEntity>().ToLookup(_ => string.Empty);

		IReadOnlyList<GuideEntity> guides = await repositoryService.Guides
			.GetListAsync(new Query<GuideEntity> { Where = guide => guide.Channel != null && channels.Contains(guide.Channel) }, cancellationToken)
			.ConfigureAwait(false);

		return guides.ToLookup(guide => guide.Channel!, StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Builds a mapping from the guides of the channel, the first guide that is known wins.
	/// </summary>
	private static GuideMappingResponse Prefill(EntryModel entry, string key, ILookup<string, GuideEntity> guidesByChannel)
	{
		string channel = GetChannelKey(entry);
		GuideEntity? guide = string.IsNullOrWhiteSpace(channel)
			? null
			: guidesByChannel[channel]
				.OrderBy(guide => MatchesFeed(guide, entry.Feed) ? 0 : 1)
				.ThenBy(guide => guide.Site, StringComparer.OrdinalIgnoreCase)
				.FirstOrDefault();

		return new GuideMappingResponse
		{
			EntryKey = key,
			Title = entry.Title,
			Site = guide?.Site,
			SiteId = guide?.SiteId,
			Lang = guide?.Lang,
			XmltvId = entry.Metadata.TvgId,
			DisplayName = entry.Title,
			IsStored = false
		};
	}

	private static GuideMappingResponse ToResponse(GuideMappingEntity entity, EntryModel entry, string key) => new()
	{
		EntryKey = key,
		Title = entry.Title,
		Site = entity.Site,
		SiteId = entity.SiteId,
		Lang = entity.Lang,
		XmltvId = entity.XmltvId,
		DisplayName = entity.DisplayName ?? entry.Title,
		IsStored = true
	};

	private static bool MatchesFeed(GuideEntity guide, string? feed)
		=> feed is not null && feed.Equals(guide.Feed, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Gets what identifies the entry: the <c>tvg-id</c>, or the URL if it has none.
	/// </summary>
	private static string GetEntryKey(EntryModel entry)
		=> string.IsNullOrWhiteSpace(entry.Metadata.TvgId) ? entry.FilePath : entry.Metadata.TvgId.Trim();

	/// <summary>
	/// Gets the iptv-org channel of the entry: the one it was added from, otherwise the
	/// <c>tvg-id</c>, which iptv-org uses as the channel identifier.
	/// </summary>
	private static string GetChannelKey(EntryModel entry)
	{
		if (!string.IsNullOrWhiteSpace(entry.Channel))
			return entry.Channel;

		string? tvgId = entry.Metadata.TvgId;

		// A feed is written as "Channel@Feed", the guides know the channel.
		return string.IsNullOrWhiteSpace(tvgId) ? string.Empty : tvgId.Split('@')[0].Trim();
	}

	private static bool HoldsSomething(GuideMappingResponse mapping)
		=> !string.IsNullOrWhiteSpace(mapping.Site)
		|| !string.IsNullOrWhiteSpace(mapping.SiteId)
		|| !string.IsNullOrWhiteSpace(mapping.Lang)
		|| !string.IsNullOrWhiteSpace(mapping.XmltvId);

	private static string? Clean(string? value)
		=> string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}