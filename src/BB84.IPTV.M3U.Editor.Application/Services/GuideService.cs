// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
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
	/// <summary>
	/// The number of channel identifiers per <c>IN</c> clause, so the parameter limit of SQLite is
	/// never reached.
	/// </summary>
	private const int ChannelChunkSize = 500;

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
		HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);

		foreach (EntryModel entry in playlist.Entries)
		{
			string key = GetEntryKey(entry);

			if (string.IsNullOrWhiteSpace(key))
				continue;

			// A playlist can hold the same tvg-id more than once. The mapping belongs to the key,
			// not to the entry, so the first entry of a key gets the row and the ones after it are
			// left out: a further row would be an editable copy that cannot be stored next to the
			// first one, and it would be written into the channels.xml twice.
			if (!keys.Add(key))
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

		// One mapping per key, whatever the caller passes: the key is unique per playlist, so a
		// playlist that holds the same tvg-id twice would otherwise break the insert.
		List<GuideMappingEntity> entities = [.. mappings
			.Where(HoldsSomething)
			.GroupBy(mapping => mapping.EntryKey, StringComparer.OrdinalIgnoreCase)
			.Select(group => group.First())
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

	public async Task<IReadOnlyList<GuideOptionResponse>> GetOptionsAsync(string channel, string? feed = null, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channel);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<GuideEntity> guides = await repositoryService.Guides
			.GetListAsync(new Query<GuideEntity> { Where = guide => guide.Channel == channel }, cancellationToken)
			.ConfigureAwait(false);

		return [.. OrderCandidates(guides, feed).Select(guide => ToOption(guide))];
	}

	public async Task<IReadOnlyList<GuideSiteResponse>> GetSitesAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		// Only the two columns the list is built from are read, the table holds tens of thousands of rows.
		IReadOnlyList<SiteChannel> pairs = await repositoryService.Guides
			.GetListAsync(guide => new SiteChannel(guide.Site, guide.Channel), new Query<GuideEntity>(), cancellationToken)
			.ConfigureAwait(false);

		return [.. pairs
			.GroupBy(pair => pair.Site, StringComparer.OrdinalIgnoreCase)
			.Select(group => new GuideSiteResponse
			{
				Site = group.Key,
				ChannelCount = group
					.Select(pair => pair.Channel)
					.Where(channel => !string.IsNullOrWhiteSpace(channel))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.Count(),
				GuideCount = group.Count()
			})
			.OrderBy(site => site.Site, StringComparer.OrdinalIgnoreCase)];
	}

	public async Task<IPagedList<GuideOptionResponse>> SearchSiteChannelsAsync(GuideSiteSearchRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		string site = request.Site;
		string? text = Clean(request.SearchText);

		Expression<Func<GuideEntity, bool>> filter = guide => guide.Site == site;

		if (text is not null)
		{
			filter = guide => guide.Site == site
				&& ((guide.Channel != null && guide.Channel.Contains(text)) || guide.SiteId.Contains(text) || guide.SiteName.Contains(text));
		}

		int total = await repositoryService.Guides
			.CountAsync(new Query<GuideEntity> { Where = filter }, cancellationToken)
			.ConfigureAwait(false);

		// Only the page is read, a site of the catalog can hold thousands of guides.
		IReadOnlyList<GuideEntity> page = await repositoryService.Guides
			.GetListAsync(
				new Query<GuideEntity>
				{
					Where = filter,
					OrderBy = query => query.OrderBy(guide => guide.Channel).ThenBy(guide => guide.Feed),
					Skip = request.Skip,
					Take = request.PageSize
				},
				cancellationToken)
			.ConfigureAwait(false);

		// What the catalog knows about the channels of the page, so a row can be recognised.
		List<string> channels = [.. page
			.Select(guide => guide.Channel)
			.OfType<string>()
			.Distinct(StringComparer.OrdinalIgnoreCase)];

		Dictionary<string, ChannelInfo> channelsById = await LoadChannelsAsync(repositoryService, channels, cancellationToken)
			.ConfigureAwait(false);

		IEnumerable<GuideOptionResponse> options = page
			.Select(guide => ToOption(guide, Lookup(channelsById, guide.Channel)));

		return new PagedList<GuideOptionResponse>(options, total, request.PageNumber, request.PageSize);
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
			: OrderCandidates(guidesByChannel[channel], entry.Feed).FirstOrDefault();

		return new GuideMappingResponse
		{
			EntryKey = key,
			Title = entry.Title,
			Site = guide?.Site,
			SiteId = guide?.SiteId,
			Lang = guide?.Lang,
			XmltvId = entry.Metadata.TvgId,
			DisplayName = entry.Title,
			Channel = ChannelOrNull(entry),
			Feed = entry.Feed,
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
		Channel = ChannelOrNull(entry),
		Feed = entry.Feed,
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

	/// <summary>
	/// Orders the guides of a channel the way a mapping is prefilled: the one of the feed first, then
	/// by site, so the picker and the prefill can never disagree.
	/// </summary>
	private static IEnumerable<GuideEntity> OrderCandidates(IEnumerable<GuideEntity> guides, string? feed)
		=> guides
			.OrderBy(guide => MatchesFeed(guide, feed) ? 0 : 1)
			.ThenBy(guide => guide.Site, StringComparer.OrdinalIgnoreCase);

	private static GuideOptionResponse ToOption(GuideEntity guide, ChannelInfo? channel = null) => new()
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
	/// Gets the iptv-org channel of the entry, <see langword="null"/> if it names none.
	/// </summary>
	private static string? ChannelOrNull(EntryModel entry)
		=> Clean(GetChannelKey(entry));

	/// <summary>
	/// Loads what the catalog knows about the given channels, in chunks, so the query stays within
	/// the parameter limit of SQLite.
	/// </summary>
	private static async Task<Dictionary<string, ChannelInfo>> LoadChannelsAsync(IRepositoryService repositoryService, IReadOnlyList<string> channels, CancellationToken cancellationToken)
	{
		Dictionary<string, ChannelInfo> channelsById = new(StringComparer.OrdinalIgnoreCase);

		foreach (string[] chunk in channels.Chunk(ChannelChunkSize))
		{
			IReadOnlyList<ChannelInfo> loaded = await repositoryService.Channels
				.GetListAsync(
					channel => new ChannelInfo(channel.Channel, channel.Name, channel.Country),
					new Query<ChannelEntity> { Where = channel => chunk.Contains(channel.Channel) },
					cancellationToken)
				.ConfigureAwait(false);

			foreach (ChannelInfo info in loaded)
				channelsById[info.Channel] = info;
		}

		return channelsById;
	}

	private static ChannelInfo? Lookup(Dictionary<string, ChannelInfo> channelsById, string? channel)
		=> channel is not null && channelsById.TryGetValue(channel, out ChannelInfo? info) ? info : null;

	/// <summary>
	/// The two columns the site list is built from.
	/// </summary>
	private sealed record SiteChannel(string Site, string? Channel);

	/// <summary>
	/// What the catalog knows about a channel of the site browser.
	/// </summary>
	private sealed record ChannelInfo(string Channel, string Name, string Country);

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}
