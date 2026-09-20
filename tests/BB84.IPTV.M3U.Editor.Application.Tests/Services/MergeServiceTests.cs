// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class MergeServiceTests
{
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly MergeService _sut;

	public MergeServiceTests()
	{
		_sut = new MergeService(_playlistServiceMock.Object);

		SetupPlaylist(1, CreatePlaylist(
			CreateEntry("First", "http://one", "ARD.de", "News"),
			CreateEntry("Second", "http://two", "ZDF.de", "News")));

		SetupPlaylist(2, CreatePlaylist(
			CreateEntry("Second again", "http://two-mirror", "ZDF.de", "Public"),
			CreateEntry("Third", "http://three", "RTL.de", "Private")));

		_playlistServiceMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(42);
	}

	[TestMethod]
	public async Task PreviewAsyncShouldAppendInTheRequestedOrder()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [2, 1] })
			.ConfigureAwait(false);

		Assert.AreEqual(2, preview.SourceCount);
		Assert.AreEqual(4, preview.SourceEntryCount);
		Assert.AreEqual(0, preview.DuplicateCount);
		Assert.AreSequenceEqual(["Second again", "Third", "First", "Second"], preview.Playlist.Entries.Select(entry => entry.Title));
	}

	[TestMethod]
	public async Task PreviewAsyncShouldKeepTheHeaderOfTheFirstSource()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 2] })
			.ConfigureAwait(false);

		Assert.AreEqual("https://tvg.example/1", preview.Playlist.UrlTvg);
		Assert.AreEqual(100, preview.Playlist.Cache);
	}

	[TestMethod]
	public async Task PreviewAsyncShouldDropDuplicatesByTvgIdAndKeepTheFirst()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 2], DuplicateMode = MergeDuplicateMode.ByTvgId })
			.ConfigureAwait(false);

		Assert.AreEqual(1, preview.DuplicateCount);
		Assert.AreSequenceEqual(["First", "Second", "Third"], preview.Playlist.Entries.Select(entry => entry.Title));
		Assert.AreEqual("http://two", preview.Playlist.Entries.ElementAt(1).FilePath);
	}

	[TestMethod]
	public async Task PreviewAsyncShouldDropDuplicatesByTvgIdAndKeepTheLast()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest
			{
				PlaylistIds = [1, 2],
				DuplicateMode = MergeDuplicateMode.ByTvgId,
				DuplicateResolution = MergeDuplicateResolution.KeepLast
			})
			.ConfigureAwait(false);

		Assert.AreEqual(1, preview.DuplicateCount);
		Assert.AreSequenceEqual(["First", "Second again", "Third"], preview.Playlist.Entries.Select(entry => entry.Title));
	}

	[TestMethod]
	public async Task PreviewAsyncShouldDropDuplicatesByUrl()
	{
		SetupPlaylist(3, CreatePlaylist(CreateEntry("Copy of first", "http://one", "Other.de", "News")));

		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 3], DuplicateMode = MergeDuplicateMode.ByUrl })
			.ConfigureAwait(false);

		Assert.AreEqual(1, preview.DuplicateCount);
		Assert.AreSequenceEqual(["First", "Second"], preview.Playlist.Entries.Select(entry => entry.Title));
	}

	[TestMethod]
	public async Task PreviewAsyncShouldDropDuplicatesByTvgIdOrUrl()
	{
		SetupPlaylist(3, CreatePlaylist(
			CreateEntry("Same url", "http://one", "Other.de", "News"),
			CreateEntry("Same id", "http://elsewhere", "ZDF.de", "News")));

		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 3], DuplicateMode = MergeDuplicateMode.ByTvgIdOrUrl })
			.ConfigureAwait(false);

		Assert.AreEqual(2, preview.DuplicateCount);
		Assert.AreSequenceEqual(["First", "Second"], preview.Playlist.Entries.Select(entry => entry.Title));
	}

	[TestMethod]
	public async Task PreviewAsyncShouldKeepEntriesWithoutAKey()
	{
		SetupPlaylist(3, CreatePlaylist(
			CreateEntry("No id", "http://a", null, "News"),
			CreateEntry("No id either", "http://b", null, "News")));

		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [3], DuplicateMode = MergeDuplicateMode.ByTvgId })
			.ConfigureAwait(false);

		Assert.AreEqual(0, preview.DuplicateCount);
		Assert.HasCount(2, preview.Playlist.Entries.ToList());
	}

	[TestMethod]
	public async Task PreviewAsyncShouldRenameAndClearGroups()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest
			{
				PlaylistIds = [1, 2],
				GroupMappings = new Dictionary<string, string> { ["News"] = "Nachrichten", ["Private"] = string.Empty }
			})
			.ConfigureAwait(false);

		Assert.AreSequenceEqual(["Nachrichten", "Nachrichten", "Public", null!], preview.Playlist.Entries.Select(entry => entry.Metadata.GroupTitle));
		Assert.AreSequenceEqual(["", "Nachrichten", "Public"], preview.Groups);
	}

	[TestMethod]
	public async Task PreviewAsyncShouldSkipAPlaylistThatIsGone()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 99] })
			.ConfigureAwait(false);

		Assert.AreEqual(1, preview.SourceCount);
		Assert.HasCount(2, preview.Playlist.Entries.ToList());
	}

	[TestMethod]
	public async Task PreviewAsyncShouldNotTouchTheSources()
	{
		MergePreviewResponse preview = await _sut
			.PreviewAsync(new MergeRequest { PlaylistIds = [1, 2], GroupMappings = new Dictionary<string, string> { ["News"] = "Renamed" } })
			.ConfigureAwait(false);

		preview.Playlist.Entries.First().Title = "Changed in the preview";

		IPlaylist source = (await _playlistServiceMock.Object.LoadAsync(1).ConfigureAwait(false))!;

		Assert.AreEqual("First", source.Entries.First().Title);
		Assert.AreEqual("News", source.Entries.First().Metadata.GroupTitle);
		_playlistServiceMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task MergeAsyncShouldStoreTheResultAsANewPlaylist()
	{
		int id = await _sut
			.MergeAsync(new MergeRequest { PlaylistIds = [1, 2], Name = " Everything " })
			.ConfigureAwait(false);

		Assert.AreEqual(42, id);
		_playlistServiceMock.Verify(x => x.CreateAsync(
			"Everything",
			It.Is<IPlaylist>(playlist => playlist.Entries.Count() == 4),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task MergeAsyncShouldUseTheDefaultNameWhenNoneIsGiven()
	{
		_ = await _sut
			.MergeAsync(new MergeRequest { PlaylistIds = [1, 2] })
			.ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.CreateAsync(
			It.Is<string>(name => !string.IsNullOrWhiteSpace(name)),
			It.IsAny<IPlaylist>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task MergeAsyncShouldFailWithoutSources()
	{
		_ = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
			() => _sut.MergeAsync(new MergeRequest { PlaylistIds = [99] })).ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	/// <summary>
	/// Lets the service return a fresh copy per call, like a load from the database does.
	/// </summary>
	private void SetupPlaylist(int id, Func<PlaylistModel> factory)
		=> _playlistServiceMock.Setup(x => x.LoadAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(factory);

	private static Func<PlaylistModel> CreatePlaylist(params EntryModel[] entries)
		=> () => new PlaylistModel(
			new PlaylistModel { UrlTvg = "https://tvg.example/1", Cache = 100, Deinterlace = Deinterlace.Blend, Refresh = 60 },
			entries.Select(entry => new EntryModel(entry)));

	private static EntryModel CreateEntry(string title, string url, string? tvgId, string? groupTitle)
		=> new(title, url, metadata: new MetadataModel { TvgId = tvgId, GroupTitle = groupTitle });
}