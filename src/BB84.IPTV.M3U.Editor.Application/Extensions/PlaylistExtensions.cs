using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

/// <summary>
/// Maps playlists between the domain models and the database entities.
/// </summary>
internal static class PlaylistExtensions
{
	/// <summary>
	/// Creates a new playlist entity with its entries from the <paramref name="playlist"/>.
	/// </summary>
	/// <param name="playlist">The playlist to map.</param>
	/// <param name="name">The name of the playlist.</param>
	/// <returns>The new playlist entity.</returns>
	internal static PlaylistEntity ToEntity(this IPlaylist playlist, string name)
	{
		PlaylistEntity entity = new() { Name = name };
		entity.Apply(playlist);
		entity.Entries = [.. playlist.ToEntryEntities()];
		return entity;
	}

	/// <summary>
	/// Copies the header values of the <paramref name="playlist"/> to the <paramref name="entity"/>.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="playlist">The playlist to copy from.</param>
	internal static void Apply(this PlaylistEntity entity, IPlaylist playlist)
	{
		entity.UrlTvg = playlist.UrlTvg;
		entity.Cache = playlist.Cache;
		entity.Deinterlace = playlist.Deinterlace;
		entity.Refresh = playlist.Refresh;
		entity.AdditionalAttributes = playlist.AdditionalAttributes;
	}

	/// <summary>
	/// Creates the entry entities of the <paramref name="playlist"/>, numbered by their position.
	/// </summary>
	/// <param name="playlist">The playlist to map.</param>
	/// <param name="playlistId">The identifier of the playlist the entries belong to.</param>
	/// <returns>The entry entities in playlist order.</returns>
	internal static IEnumerable<PlaylistEntryEntity> ToEntryEntities(this IPlaylist playlist, int playlistId = default)
		=> playlist.Entries.Select((entry, position) => entry.ToEntity(position, playlistId));

	/// <summary>
	/// Creates a playlist model from the <paramref name="entity"/>, with the entries ordered by position.
	/// </summary>
	/// <param name="entity">The entity to map, its entries must be loaded.</param>
	/// <returns>The playlist model.</returns>
	internal static PlaylistModel ToModel(this PlaylistEntity entity)
	{
		PlaylistModel header = new()
		{
			UrlTvg = entity.UrlTvg,
			Cache = entity.Cache,
			Deinterlace = entity.Deinterlace,
			Refresh = entity.Refresh,
			AdditionalAttributes = entity.AdditionalAttributes
		};

		return new PlaylistModel(header, entity.Entries.OrderBy(e => e.Position).Select(e => e.ToModel()));
	}

	private static PlaylistEntryEntity ToEntity(this EntryModel entry, int position, int playlistId) => new()
	{
		PlaylistId = playlistId,
		Position = position,
		Duration = entry.Duration,
		Title = entry.Title,
		Url = entry.FilePath,
		Grouping = entry.Grouping,
		Directives = entry.Directives,
		Censored = entry.Metadata.Censored,
		TvgId = entry.Metadata.TvgId,
		TvgName = entry.Metadata.TvgName,
		TvgLogo = entry.Metadata.TvgLogo,
		GroupId = entry.Metadata.GroupId,
		GroupTitle = entry.Metadata.GroupTitle,
		AdditionalAttributes = entry.Metadata.AdditionalAttributes,
		Channel = entry.Channel,
		Feed = entry.Feed
	};

	private static EntryModel ToModel(this PlaylistEntryEntity entity)
	{
		MetadataModel metadata = new()
		{
			Censored = entity.Censored,
			TvgId = entity.TvgId,
			TvgName = entity.TvgName,
			TvgLogo = entity.TvgLogo,
			GroupId = entity.GroupId,
			GroupTitle = entity.GroupTitle,
			AdditionalAttributes = entity.AdditionalAttributes
		};

		return new EntryModel(entity.Title, entity.Url, entity.Duration, entity.Grouping, metadata)
		{
			Directives = entity.Directives,
			Channel = entity.Channel,
			Feed = entity.Feed
		};
	}
}