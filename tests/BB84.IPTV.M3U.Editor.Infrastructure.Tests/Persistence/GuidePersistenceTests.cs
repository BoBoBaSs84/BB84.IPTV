// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Maps the entries of a playlist in a real SQLite database and writes the <c>channels.xml</c>.
/// </summary>
[TestClass]
public sealed class GuidePersistenceTests
{
	private SqliteTestDatabase _database = default!;
	private IGuideService _sut = default!;
	private IPlaylistService _playlistService = default!;
	private string _directory = default!;
	private int _playlistId;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_directory = Directory.CreateTempSubdirectory().FullName;
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		_sut = _database.Services.GetRequiredService<IGuideService>();
		_playlistService = _database.Services.GetRequiredService<IPlaylistService>();

		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Guides.CreateAsync(
			[
				new GuideEntity { Channel = "DasErste.de", Feed = null, Site = "example.com", SiteId = "100", SiteName = "Example", Lang = "de" },
				new GuideEntity { Channel = "ZDF.de", Feed = "HD", Site = "hd.example.com", SiteId = "200", SiteName = "Example HD", Lang = "de" },
				new GuideEntity { Channel = "ZDF.de", Feed = null, Site = "example.com", SiteId = "201", SiteName = "Example", Lang = "de" }
			]).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);

		_playlistId = await _playlistService.CreateAsync("Mine", new PlaylistModel(new PlaylistModel(),
		[
			new EntryModel("Das Erste", "https://example.com/ard.m3u8", metadata: new MetadataModel { TvgId = "DasErste.de" }),
			new EntryModel("ZDF HD", "https://example.com/zdf.m3u8", metadata: new MetadataModel { TvgId = "ZDF.de@HD" }) { Channel = "ZDF.de", Feed = "HD" },
			new EntryModel("Local camera", "rtsp://192.168.12.1:554")
		])).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
	{
		_database.Dispose();
		Directory.Delete(_directory, true);
	}

	[TestMethod]
	public async Task GetMappingsAsyncShouldPrefillFromTheImportedGuides()
	{
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		Assert.HasCount(3, mappings);

		Assert.AreEqual("example.com", mappings[0].Site);
		Assert.AreEqual("100", mappings[0].SiteId);
		Assert.AreEqual("DasErste.de", mappings[0].XmltvId);
		Assert.IsTrue(mappings[0].IsComplete);

		// The guide of the feed the entry belongs to wins over the one of the channel.
		Assert.AreEqual("hd.example.com", mappings[1].Site);
		Assert.AreEqual("200", mappings[1].SiteId);

		// Nothing is known about a custom channel, it is mapped by hand.
		Assert.AreEqual("Local camera", mappings[2].Title);
		Assert.IsNull(mappings[2].Site);
		Assert.IsFalse(mappings[2].IsComplete);
		Assert.AreEqual("rtsp://192.168.12.1:554", mappings[2].EntryKey);
	}

	[TestMethod]
	public async Task SaveMappingsAsyncShouldWinOverThePrefilledOnes()
	{
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		int saved = await _sut.SaveMappingsAsync(_playlistId,
		[
			Override(mappings[0], site: "mine.example", siteId: "999"),
			mappings[1],
			Override(mappings[2], site: "camera.example", siteId: "1", xmltvId: "Camera.local")
		]).ConfigureAwait(false);

		IReadOnlyList<GuideMappingResponse> stored = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		Assert.AreEqual(3, saved);
		Assert.AreEqual("mine.example", stored[0].Site);
		Assert.AreEqual("999", stored[0].SiteId);
		Assert.IsTrue(stored[0].IsStored);
		Assert.AreEqual("Camera.local", stored[2].XmltvId);
		Assert.IsTrue(stored[2].IsComplete);
	}

	[TestMethod]
	public async Task SaveMappingsAsyncShouldSurviveSavingThePlaylist()
	{
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);
		_ = await _sut.SaveMappingsAsync(_playlistId, [Override(mappings[0], site: "mine.example", siteId: "999")]).ConfigureAwait(false);

		// Saving a playlist replaces all its entries, the mappings must still be found.
		Domain.Abstractions.Models.IPlaylist playlist = (await _playlistService.LoadAsync(_playlistId).ConfigureAwait(false))!;
		_ = await _playlistService.UpdateAsync(_playlistId, "Mine", playlist).ConfigureAwait(false);

		IReadOnlyList<GuideMappingResponse> stored = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		Assert.AreEqual("mine.example", stored[0].Site);
		Assert.IsTrue(stored[0].IsStored);
	}

	[TestMethod]
	public async Task SaveMappingsAsyncShouldReplaceWhatWasStoredBefore()
	{
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);
		_ = await _sut.SaveMappingsAsync(_playlistId, mappings).ConfigureAwait(false);

		// A row the user emptied completely, language included, is not worth storing.
		GuideMappingResponse emptied = new() { EntryKey = mappings[0].EntryKey, Title = mappings[0].Title };

		int saved = await _sut.SaveMappingsAsync(_playlistId, [emptied]).ConfigureAwait(false);

		IReadOnlyList<GuideMappingResponse> stored = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		Assert.AreEqual(0, saved, "A row without any value is not stored.");
		Assert.IsFalse(stored.Any(mapping => mapping.IsStored));
	}

	[TestMethod]
	public async Task ExportAsyncShouldWriteTheChannelsFile()
	{
		string filePath = Path.Combine(_directory, "channels.xml");

		int written = await _sut.ExportAsync(_playlistId, filePath).ConfigureAwait(false);

		string content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		Assert.AreEqual(2, written, "The custom channel is not mapped, so it is not written.");
		Assert.Contains("<channel site=\"example.com\" site_id=\"100\" lang=\"de\" xmltv_id=\"DasErste.de\">Das Erste</channel>", content);
		Assert.Contains("site_id=\"200\"", content);
		Assert.DoesNotContain("Local camera", content);
	}

	[TestMethod]
	public async Task ExportAsyncShouldWriteTheMappingsItIsGiven()
	{
		string filePath = Path.Combine(_directory, "channels.xml");
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		int written = await _sut
			.ExportAsync(_playlistId, filePath, [Override(mappings[2], site: "camera.example", siteId: "1", xmltvId: "Camera.local")])
			.ConfigureAwait(false);

		string content = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);

		Assert.AreEqual(1, written);
		Assert.Contains("xmltv_id=\"Camera.local\"", content);
		Assert.DoesNotContain("DasErste.de", content);
	}

	[TestMethod]
	public async Task GetMappingsAsyncShouldCarryTheChannelOfTheEntry()
	{
		IReadOnlyList<GuideMappingResponse> mappings = await _sut.GetMappingsAsync(_playlistId).ConfigureAwait(false);

		// What the guides of a row are looked up with, also for an entry that only holds a tvg-id.
		Assert.AreEqual("DasErste.de", mappings[0].Channel);
		Assert.IsNull(mappings[0].Feed);
		Assert.AreEqual("ZDF.de", mappings[1].Channel);
		Assert.AreEqual("HD", mappings[1].Feed);
		Assert.IsNull(mappings[2].Channel, "A custom channel names none.");
	}

	private static GuideMappingResponse Override(GuideMappingResponse mapping, string? site = null, string? siteId = null, string? xmltvId = null) => new()
	{
		EntryKey = mapping.EntryKey,
		Title = mapping.Title,
		Site = site,
		SiteId = siteId,
		Lang = mapping.Lang,
		XmltvId = xmltvId ?? mapping.XmltvId,
		DisplayName = mapping.DisplayName
	};
}
