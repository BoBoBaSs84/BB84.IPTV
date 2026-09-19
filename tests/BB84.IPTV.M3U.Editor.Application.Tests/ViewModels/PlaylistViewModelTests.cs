using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class PlaylistViewModelTests
{
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly Mock<IFileService> _fileServiceMock = new();
	private readonly PlaylistViewModel _sut;

	public PlaylistViewModelTests()
		=> _sut = new PlaylistViewModel(_playlistServiceMock.Object, _fileServiceMock.Object);

	[TestMethod]
	public async Task LoadAsyncShouldOpenThePlaylistWithoutMarkingItDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first"), new EntryModel("Second", "http://second")).ConfigureAwait(false);

		Assert.IsTrue(_sut.HasPlaylist);
		Assert.AreEqual(1, _sut.PlaylistId);
		Assert.AreEqual("My Playlist", _sut.Name);
		Assert.IsFalse(_sut.IsDirty);
		Assert.IsTrue(_sut.IsValid);
		Assert.HasCount(2, _sut.Entries);
		Assert.AreSame(_sut.Entries[0], _sut.SelectedEntry);
	}

	[TestMethod]
	public async Task LoadAsyncShouldClearTheEditorWhenThePlaylistDoesNotExist()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);
		_playlistServiceMock.Setup(x => x.LoadAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((IPlaylist?)null);

		bool loaded = await _sut.LoadAsync(2, "Missing").ConfigureAwait(false);

		Assert.IsFalse(loaded);
		Assert.IsFalse(_sut.HasPlaylist);
		Assert.IsEmpty(_sut.Entries);
	}

	[TestMethod]
	public async Task AddEntryShouldMarkDirtyAndRequireAnUrl()
	{
		await LoadAsync().ConfigureAwait(false);

		_sut.AddEntry();

		Assert.IsTrue(_sut.IsDirty);
		Assert.AreSame(_sut.Entries[0], _sut.SelectedEntry);
		Assert.IsFalse(_sut.IsValid);
		Assert.IsFalse(_sut.CanSave);
		Assert.IsNotNull(_sut.ValidationMessage);

		_sut.Entries[0].FilePath = "rtsp://192.168.12.1:554";

		Assert.IsTrue(_sut.IsValid);
		Assert.IsTrue(_sut.CanSave);
	}

	[TestMethod]
	public async Task ChangingEntryOrMetadataShouldMarkDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);

		_sut.Entries[0].Metadata.TvgId = "DasErste.de";

		Assert.IsTrue(_sut.IsDirty);
	}

	[TestMethod]
	public async Task ChangingHeaderShouldMarkDirty()
	{
		await LoadAsync().ConfigureAwait(false);

		_sut.UrlTvg = "https://epg.example";

		Assert.IsTrue(_sut.IsDirty);
	}

	[TestMethod]
	public async Task EmptyNameShouldBeInvalid()
	{
		await LoadAsync().ConfigureAwait(false);

		_sut.Name = " ";

		Assert.IsFalse(_sut.IsValid);
		Assert.IsFalse(_sut.CanSave);
	}

	[TestMethod]
	public async Task ClearShouldDetachRemovedEntries()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);
		IEntry removed = _sut.Entries[0];

		_sut.Clear();
		removed.Title = "Changed";
		removed.Metadata.TvgName = "Changed";

		Assert.IsFalse(_sut.IsDirty);
		Assert.IsFalse(_sut.HasPlaylist);
		Assert.IsEmpty(_sut.Entries);
	}

	[TestMethod]
	public async Task MoveSelectedEntryShouldMoveEntryAndMarkDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first"), new EntryModel("Second", "http://second")).ConfigureAwait(false);
		IEntry first = _sut.Entries[0];

		_sut.MoveSelectedEntry(1);

		Assert.AreSame(first, _sut.Entries[1]);
		Assert.AreSame(first, _sut.SelectedEntry);
		Assert.IsTrue(_sut.IsDirty);
	}

	[TestMethod]
	public async Task SaveAsyncShouldStoreNameAndEntriesAndClearDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);
		_sut.Name = "Renamed";
		IPlaylist? saved = null;
		_playlistServiceMock.Setup(x => x.UpdateAsync(1, "Renamed", It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()))
			.Callback<int, string, IPlaylist, CancellationToken>((id, name, playlist, token) => saved = playlist)
			.ReturnsAsync(true);

		bool result = await _sut.SaveAsync().ConfigureAwait(false);

		Assert.IsTrue(result);
		Assert.IsFalse(_sut.IsDirty);
		Assert.AreEqual("http://first", saved!.Entries.Single().FilePath);
	}

	[TestMethod]
	public async Task SaveAsyncShouldNotStoreAnInvalidPlaylist()
	{
		await LoadAsync().ConfigureAwait(false);
		_sut.AddEntry();

		bool result = await _sut.SaveAsync().ConfigureAwait(false);

		Assert.IsFalse(result);
		Assert.IsTrue(_sut.IsDirty);
		_playlistServiceMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task MergeFileAsyncShouldAppendEntriesAndMarkDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);
		Mock<IPlaylist> merged = new();
		merged.SetupGet(x => x.Entries).Returns([new EntryModel("Second", "http://second"), new EntryModel("Third", "http://third")]);
		_fileServiceMock.Setup(x => x.LoadAsync("other.m3u", It.IsAny<CancellationToken>())).ReturnsAsync(merged.Object);

		int appended = await _sut.MergeFileAsync("other.m3u").ConfigureAwait(false);

		Assert.AreEqual(2, appended);
		Assert.HasCount(3, _sut.Entries);
		Assert.IsTrue(_sut.IsDirty);
	}

	private async Task LoadAsync(params EntryModel[] entries)
	{
		_playlistServiceMock.Setup(x => x.LoadAsync(1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PlaylistModel(new PlaylistModel(), entries));

		await _sut.LoadAsync(1, "My Playlist").ConfigureAwait(false);
	}
}