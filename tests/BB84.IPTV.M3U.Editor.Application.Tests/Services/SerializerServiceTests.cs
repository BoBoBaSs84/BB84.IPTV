using System.Text;

using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class SerializerServiceTests
{
	private readonly SerializerService _sut;

	public SerializerServiceTests()
		=> _sut = new();

	[TestMethod]
	public void DeserializeShouldThrowInvalidDataExceptionWhenFileIsNotValidM3U()
	{
		string[] invalidFileLines = ["#EXTINF:123, Sample artist - Sample title", "http://example.com/stream"];

		Assert.Throws<InvalidDataException>(() => _sut.Deserialize(invalidFileLines));
	}

	[TestMethod]
	public void DeserializeShouldParseHeaderAttributesAndEntries()
	{
		string[] fileLines =
		[
			"#EXTM3U url-tvg=\"https://tvg.example\" refresh=\"60\" cache=\"120\"",
			"#EXTINF:123 censored=\"1\" tvg-id=\"id1\" tvg-name=\"name1\" tvg-logo=\"logo1\" group_id=\"group1\" group-title=\"News\",Channel 1",
			"#EXTGRP:Favorites",
			"https://example.com/stream1"
		];

		var result = _sut.Deserialize(fileLines);

		Assert.AreEqual("https://tvg.example", result.UrlTvg);
		Assert.AreEqual(60, result.Refresh);
		Assert.AreEqual(120, result.Cache);

		EntryModel entry = result.Entries.Single();
		Assert.AreEqual(123, entry.Duration);
		Assert.AreEqual("Channel 1", entry.Title);
		Assert.AreEqual("Favorites", entry.Grouping);
		Assert.AreEqual("https://example.com/stream1", entry.FilePath);
		Assert.IsTrue(entry.Metadata.Censored);
		Assert.AreEqual("id1", entry.Metadata.TvgId);
		Assert.AreEqual("name1", entry.Metadata.TvgName);
		Assert.AreEqual("logo1", entry.Metadata.TvgLogo);
		Assert.AreEqual("group1", entry.Metadata.GroupId);
		Assert.AreEqual("News", entry.Metadata.GroupTitle);
	}

	[TestMethod]
	public void DeserializeShouldIgnoreInvalidHeaderNumbers()
	{
		string[] fileLines =
		[
			"#EXTM3U refresh=\"abc\" cache=\"xyz\"",
			"#EXTINF:10,Channel",
			"https://example.com/stream"
		];

		var result = _sut.Deserialize(fileLines);

		Assert.AreEqual(0, result.Refresh);
		Assert.AreEqual(0, result.Cache);
	}

	[TestMethod]
	public void DeserializeShouldUseEmptyTitleWhenNoCommaIsProvided()
	{
		string[] fileLines =
		[
			"#EXTM3U",
			"#EXTINF:20 tvg-id=\"id1\"",
			"https://example.com/stream"
		];

		var result = _sut.Deserialize(fileLines);

		EntryModel entry = result.Entries.Single();
		Assert.AreEqual(20, entry.Duration);
		Assert.AreEqual(string.Empty, entry.Title);
	}

	[TestMethod]
	public void DeserializeByteArrayShouldParsePlaylist()
	{
		string content = "#EXTM3U\n#EXTINF:5,Title\nhttps://example.com/stream\n";
		byte[] bytes = Encoding.UTF8.GetBytes(content);

		var result = _sut.Deserialize(bytes);

		EntryModel entry = result.Entries.Single();
		Assert.AreEqual("Title", entry.Title);
		Assert.AreEqual("https://example.com/stream", entry.FilePath);
	}

	[TestMethod]
	public void DeserializeFilePathShouldParsePlaylist()
	{
		string filePath = Path.GetTempFileName();
		try
		{
			File.WriteAllText(filePath, "#EXTM3U\r\n#EXTINF:1,Title\r\nhttps://example.com/stream\r\n");

			var result = _sut.Deserialize(filePath);

			Assert.HasCount(1, result.Entries);
		}
		finally
		{
			if (File.Exists(filePath))
				File.Delete(filePath);
		}
	}

	[TestMethod]
	public void SerializeShouldIncludePlaylistAndEntryAttributes()
	{
		PlaylistModel playlist = new()
		{
			UrlTvg = "https://tvg.example",
			Cache = 120,
			Deinterlace = Deinterlace.Blend,
			Refresh = 60
		};

		MetadataModel metadata = new()
		{
			Censored = true,
			TvgId = "id1",
			TvgName = "name1",
			TvgLogo = "logo1",
			GroupId = "group1",
			GroupTitle = "News"
		};

		EntryModel entry = new("Channel 1", "https://example.com/stream", 123, "Favorites", metadata);
		PlaylistModel fullPlaylist = new(playlist, [entry]);

		string result = _sut.Serialize(fullPlaylist);

		Assert.Contains("#EXTM3U", result);
		Assert.Contains("url-tvg=\"https://tvg.example\"", result);
		Assert.Contains("cache=\"120\"", result);
		Assert.Contains("deinterlace=\"1\"", result);
		Assert.Contains("refresh=\"60\"", result);
		Assert.Contains("#EXTINF:123 censored=\"1\" tvg-id=\"id1\" tvg-name=\"name1\" tvg-logo=\"logo1\" group_id=\"group1\" group-title=\"News\"", result);
		Assert.Contains(",Channel 1", result);
		Assert.Contains("#EXTGRP:Favorites", result);
		Assert.Contains("https://example.com/stream", result);
	}

	[TestMethod]
	public void SerializeShouldOmitOptionalPlaylistAttributesWhenNotSet()
	{
		PlaylistModel playlist = new();
		string result = _sut.Serialize(playlist);

		Assert.Contains("#EXTM3U", result);
		Assert.IsFalse(result.Contains("url-tvg=", StringComparison.Ordinal));
		Assert.IsFalse(result.Contains("cache=", StringComparison.Ordinal));
		Assert.IsFalse(result.Contains("deinterlace=", StringComparison.Ordinal));
		Assert.IsFalse(result.Contains("refresh=", StringComparison.Ordinal));
	}
}
