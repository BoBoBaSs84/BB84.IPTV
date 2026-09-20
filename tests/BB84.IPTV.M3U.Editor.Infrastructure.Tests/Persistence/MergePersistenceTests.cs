// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Merges playlists of a real SQLite database through <see cref="IMergeService"/>.
/// </summary>
[TestClass]
public sealed class MergePersistenceTests
{
	private SqliteTestDatabase _database = default!;
	private IMergeService _sut = default!;
	private IPlaylistService _playlistService = default!;
	private int _firstId;
	private int _secondId;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);
		_sut = _database.Services.GetRequiredService<IMergeService>();
		_playlistService = _database.Services.GetRequiredService<IPlaylistService>();

		_firstId = await _playlistService.CreateAsync("First", CreatePlaylist(
			CreateEntry("Das Erste", "https://example.com/ard.m3u8", "DasErste.de", "Public"),
			CreateEntry("ZDF", "https://example.com/zdf.m3u8", "ZDF.de", "Public"))).ConfigureAwait(false);

		_secondId = await _playlistService.CreateAsync("Second", CreatePlaylist(
			CreateEntry("ZDF HD", "https://example.com/zdf-hd.m3u8", "ZDF.de", "HD"),
			CreateEntry("Local camera", "rtsp://192.168.12.1:554", null, "Local"))).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task MergeAsyncShouldStoreANewPlaylistAndLeaveTheSourcesAlone()
	{
		int mergedId = await _sut
			.MergeAsync(new MergeRequest
			{
				PlaylistIds = [_firstId, _secondId],
				Name = "Everything",
				DuplicateMode = MergeDuplicateMode.ByTvgId
			})
			.ConfigureAwait(false);

		IPlaylist merged = (await _playlistService.LoadAsync(mergedId).ConfigureAwait(false))!;
		IPlaylist first = (await _playlistService.LoadAsync(_firstId).ConfigureAwait(false))!;
		IPlaylist second = (await _playlistService.LoadAsync(_secondId).ConfigureAwait(false))!;
		IPagedList<PlaylistSummaryResponse> playlists = await _playlistService.GetPlaylistsAsync().ConfigureAwait(false);

		Assert.AreNotEqual(_firstId, mergedId);
		Assert.AreNotEqual(_secondId, mergedId);
		Assert.HasCount(3, playlists);
		Assert.AreSequenceEqual(["Das Erste", "ZDF", "Local camera"], merged.Entries.Select(entry => entry.Title));
		Assert.AreSequenceEqual(["Das Erste", "ZDF"], first.Entries.Select(entry => entry.Title));
		Assert.AreSequenceEqual(["ZDF HD", "Local camera"], second.Entries.Select(entry => entry.Title));
	}

	[TestMethod]
	public async Task MergeAsyncShouldKeepTheHeaderAndRemapTheGroups()
	{
		int mergedId = await _sut
			.MergeAsync(new MergeRequest
			{
				PlaylistIds = [_firstId, _secondId],
				Name = "Remapped",
				GroupMappings = new Dictionary<string, string> { ["Public"] = "Öffentlich" }
			})
			.ConfigureAwait(false);

		IPlaylist merged = (await _playlistService.LoadAsync(mergedId).ConfigureAwait(false))!;
		IPlaylist first = (await _playlistService.LoadAsync(_firstId).ConfigureAwait(false))!;

		Assert.AreEqual("https://tvg.example", merged.UrlTvg);
		Assert.AreEqual(500, merged.Cache);
		Assert.AreSequenceEqual(["Öffentlich", "Öffentlich", "HD", "Local"], merged.Entries.Select(entry => entry.Metadata.GroupTitle));
		Assert.AreSequenceEqual(["Public", "Public"], first.Entries.Select(entry => entry.Metadata.GroupTitle));
	}

	[TestMethod]
	public async Task MergeAsyncShouldMergeAPlaylistWithItself()
	{
		int mergedId = await _sut
			.MergeAsync(new MergeRequest { PlaylistIds = [_firstId, _firstId], Name = "Doubled" })
			.ConfigureAwait(false);

		IPlaylist merged = (await _playlistService.LoadAsync(mergedId).ConfigureAwait(false))!;
		IPlaylist first = (await _playlistService.LoadAsync(_firstId).ConfigureAwait(false))!;

		Assert.HasCount(4, merged.Entries.ToList());
		Assert.HasCount(2, first.Entries.ToList());
	}

	private static PlaylistModel CreatePlaylist(params EntryModel[] entries)
		=> new(new PlaylistModel { UrlTvg = "https://tvg.example", Cache = 500, Refresh = 3600 }, entries);

	private static EntryModel CreateEntry(string title, string url, string? tvgId, string groupTitle)
		=> new(title, url, metadata: new MetadataModel { TvgId = tvgId, GroupTitle = groupTitle });
}