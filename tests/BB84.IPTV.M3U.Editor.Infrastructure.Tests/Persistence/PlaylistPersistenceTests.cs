using System.Text;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Stores playlists in a real SQLite database through <see cref="IPlaylistService"/>.
/// </summary>
[TestClass]
public sealed class PlaylistPersistenceTests
{
	private const string SampleM3u =
		"#EXTM3U url-tvg=\"https://tvg.example\" cache=\"500\" deinterlace=\"1\" refresh=\"3600\" x-tvg-url=\"https://epg.example\"\n" +
		"#EXTINF:-1 censored=\"1\" tvg-id=\"DasErste.de\" tvg-name=\"Das Erste\" tvg-logo=\"https://logo.example/ard.png\" group_id=\"1\" group-title=\"DE, Public\" tvg-country=\"DE\",Das Erste, HD\n" +
		"#EXTGRP:Favorites\n" +
		"#EXTVLCOPT:http-user-agent=Mozilla/5.0\n" +
		"https://example.com/ard.m3u8\n" +
		"#EXTINF:-1 tvg-id=\"ZDF.de\" group-title=\"DE, Public\",ZDF\n" +
		"https://example.com/zdf.m3u8\n" +
		"#EXTINF:120,Local camera\n" +
		"rtsp://192.168.12.1:554\n";

	private SqliteTestDatabase _database = default!;
	private IPlaylistService _sut = default!;
	private string _directory = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);
		_sut = _database.Services.GetRequiredService<IPlaylistService>();
		_directory = Directory.CreateTempSubdirectory().FullName;
	}

	[TestCleanup]
	public void Cleanup()
	{
		_database.Dispose();
		Directory.Delete(_directory, true);
	}

	[TestMethod]
	public async Task ImportAndExportShouldRoundTripWithoutLosingEntriesOrMetadata()
	{
		string sourcePath = WriteFile("source.m3u", SampleM3u, withByteOrderMark: true);
		string exportPath = Path.Combine(_directory, "export.m3u");

		int id = await _sut.ImportAsync(sourcePath).ConfigureAwait(false);
		bool exported = await _sut.ExportAsync(id, exportPath).ConfigureAwait(false);

		Assert.IsTrue(exported);
		Assert.AreEqual(SampleM3u, await File.ReadAllTextAsync(exportPath).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ImportAsyncShouldNameThePlaylistAfterTheFile()
	{
		string sourcePath = WriteFile("German TV.m3u8", SampleM3u);

		int id = await _sut.ImportAsync(sourcePath).ConfigureAwait(false);
		IPagedList<PlaylistSummaryResponse> playlists = await _sut.GetPlaylistsAsync().ConfigureAwait(false);

		PlaylistSummaryResponse summary = playlists.Single();
		Assert.AreEqual(id, summary.Id);
		Assert.AreEqual("German TV", summary.Name);
		Assert.AreEqual(3, summary.EntryCount);
	}

	[TestMethod]
	public async Task UpdateAsyncShouldReplaceAndReorderEntries()
	{
		int id = await _sut.ImportAsync(WriteFile("source.m3u", SampleM3u)).ConfigureAwait(false);
		IPlaylist playlist = (await _sut.LoadAsync(id).ConfigureAwait(false))!;

		// Reversed order keeps the positions but swaps the entries, which must not violate the unique position index.
		List<EntryModel> entries = [.. playlist.Entries.Reverse().Skip(1), new EntryModel("New", "https://example.com/new")];
		PlaylistModel changed = new(playlist, entries) { UrlTvg = "https://changed.example" };

		bool updated = await _sut.UpdateAsync(id, "Renamed", changed).ConfigureAwait(false);
		IPlaylist reloaded = (await _sut.LoadAsync(id).ConfigureAwait(false))!;

		Assert.IsTrue(updated);
		Assert.AreEqual("https://changed.example", reloaded.UrlTvg);
		Assert.AreEqual("Renamed", (await _sut.GetPlaylistsAsync().ConfigureAwait(false)).Single().Name);
		Assert.AreEqual("ZDF|Das Erste, HD|New", string.Join('|', reloaded.Entries.Select(e => e.Title)));
		Assert.AreEqual(3, _database.Scalar("SELECT COUNT(*) FROM PlaylistEntries"));
	}

	[TestMethod]
	public async Task RenameAsyncShouldChangeTheName()
	{
		int id = await _sut.CreateAsync("Old", new PlaylistModel()).ConfigureAwait(false);

		bool renamed = await _sut.RenameAsync(id, " New ").ConfigureAwait(false);

		Assert.IsTrue(renamed);
		Assert.AreEqual("New", (await _sut.GetPlaylistsAsync().ConfigureAwait(false)).Single().Name);
	}

	[TestMethod]
	public async Task DeleteAsyncShouldDeleteThePlaylistWithItsEntries()
	{
		int id = await _sut.ImportAsync(WriteFile("source.m3u", SampleM3u)).ConfigureAwait(false);

		bool deleted = await _sut.DeleteAsync(id).ConfigureAwait(false);

		Assert.IsTrue(deleted);
		Assert.AreEqual(0, _database.Scalar("SELECT COUNT(*) FROM Playlists"));
		Assert.AreEqual(0, _database.Scalar("SELECT COUNT(*) FROM PlaylistEntries"));
	}

	[TestMethod]
	public async Task MissingPlaylistShouldBeReported()
	{
		Assert.IsNull(await _sut.LoadAsync(42).ConfigureAwait(false));
		Assert.IsFalse(await _sut.UpdateAsync(42, "Name", new PlaylistModel()).ConfigureAwait(false));
		Assert.IsFalse(await _sut.RenameAsync(42, "Name").ConfigureAwait(false));
		Assert.IsFalse(await _sut.DeleteAsync(42).ConfigureAwait(false));
		Assert.IsFalse(await _sut.ExportAsync(42, Path.Combine(_directory, "missing.m3u")).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ResetCatalogAsyncShouldKeepPlaylists()
	{
		int id = await _sut.ImportAsync(WriteFile("source.m3u", SampleM3u)).ConfigureAwait(false);
		await _database.WithRepositoryAsync(async r =>
		{
			await r.Channels.CreateAsync(new ChannelEntity { Channel = "DasErste.de", Name = "Das Erste", Country = "DE", Owners = [], AltNames = [], Categories = [] }).ConfigureAwait(false);
			await r.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);

		int deleted = await _database.WithRepositoryAsync(r => r.ResetCatalogAsync()).ConfigureAwait(false);

		Assert.AreEqual(1, deleted);
		Assert.AreEqual(0, _database.Scalar("SELECT COUNT(*) FROM Channels"));
		Assert.HasCount(3, (await _sut.LoadAsync(id).ConfigureAwait(false))!.Entries);
	}

	private string WriteFile(string fileName, string content, bool withByteOrderMark = false)
	{
		string filePath = Path.Combine(_directory, fileName);
		File.WriteAllText(filePath, content, new UTF8Encoding(withByteOrderMark));
		return filePath;
	}
}