using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Extensions;

[TestClass]
public sealed class PlaylistExtensionsTests
{
	[TestMethod]
	public void ToEntityShouldMapHeaderAndNumberEntries()
	{
		PlaylistModel playlist = CreatePlaylist();

		PlaylistEntity entity = playlist.ToEntity("My Playlist");

		Assert.AreEqual("My Playlist", entity.Name);
		Assert.AreEqual("https://tvg.example", entity.UrlTvg);
		Assert.AreEqual(500, entity.Cache);
		Assert.AreEqual(Deinterlace.Blend, entity.Deinterlace);
		Assert.AreEqual(3600, entity.Refresh);
		Assert.AreEqual("x-tvg-url=\"https://epg.example\"", entity.AdditionalAttributes);
		Assert.AreEqual("0|1", string.Join('|', entity.Entries.Select(e => e.Position)));

		PlaylistEntryEntity first = entity.Entries.First();
		Assert.AreEqual("First", first.Title);
		Assert.AreEqual("https://example.com/1", first.Url);
		Assert.AreEqual(10, first.Duration);
		Assert.AreEqual("Favorites", first.Grouping);
		Assert.AreEqual("#EXTVLCOPT:http-referrer=https://example.com", first.Directives);
		Assert.IsTrue(first.Censored);
		Assert.AreEqual("id1", first.TvgId);
		Assert.AreEqual("name1", first.TvgName);
		Assert.AreEqual("logo1", first.TvgLogo);
		Assert.AreEqual("group1", first.GroupId);
		Assert.AreEqual("News", first.GroupTitle);
		Assert.AreEqual("tvg-country=\"DE\"", first.AdditionalAttributes);
	}

	[TestMethod]
	public void ToModelShouldRestoreAllValuesInPositionOrder()
	{
		PlaylistEntity entity = CreatePlaylist().ToEntity("My Playlist");
		entity.Entries = [.. entity.Entries.Reverse()];

		PlaylistModel model = entity.ToModel();

		Assert.AreEqual("https://tvg.example", model.UrlTvg);
		Assert.AreEqual(500, model.Cache);
		Assert.AreEqual(Deinterlace.Blend, model.Deinterlace);
		Assert.AreEqual(3600, model.Refresh);
		Assert.AreEqual("x-tvg-url=\"https://epg.example\"", model.AdditionalAttributes);
		Assert.AreEqual("First|Second", string.Join('|', model.Entries.Select(e => e.Title)));

		EntryModel first = model.Entries.First();
		Assert.AreEqual("https://example.com/1", first.FilePath);
		Assert.AreEqual(10, first.Duration);
		Assert.AreEqual("Favorites", first.Grouping);
		Assert.AreEqual("#EXTVLCOPT:http-referrer=https://example.com", first.Directives);
		Assert.IsTrue(first.Metadata.Censored);
		Assert.AreEqual("id1", first.Metadata.TvgId);
		Assert.AreEqual("name1", first.Metadata.TvgName);
		Assert.AreEqual("logo1", first.Metadata.TvgLogo);
		Assert.AreEqual("group1", first.Metadata.GroupId);
		Assert.AreEqual("News", first.Metadata.GroupTitle);
		Assert.AreEqual("tvg-country=\"DE\"", first.Metadata.AdditionalAttributes);
	}

	private static PlaylistModel CreatePlaylist()
	{
		PlaylistModel header = new()
		{
			UrlTvg = "https://tvg.example",
			Cache = 500,
			Deinterlace = Deinterlace.Blend,
			Refresh = 3600,
			AdditionalAttributes = "x-tvg-url=\"https://epg.example\""
		};

		MetadataModel metadata = new()
		{
			Censored = true,
			TvgId = "id1",
			TvgName = "name1",
			TvgLogo = "logo1",
			GroupId = "group1",
			GroupTitle = "News",
			AdditionalAttributes = "tvg-country=\"DE\""
		};

		EntryModel first = new("First", "https://example.com/1", 10, "Favorites", metadata) { Directives = "#EXTVLCOPT:http-referrer=https://example.com" };
		EntryModel second = new("Second", "https://example.com/2");

		return new PlaylistModel(header, [first, second]);
	}
}