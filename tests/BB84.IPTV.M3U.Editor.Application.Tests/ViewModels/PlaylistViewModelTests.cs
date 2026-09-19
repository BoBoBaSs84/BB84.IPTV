using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class PlaylistViewModelTests
{
	private readonly Mock<IFileService> _fileServiceMock = new();
	private readonly PlaylistViewModel _sut;

	public PlaylistViewModelTests()
		=> _sut = new PlaylistViewModel(_fileServiceMock.Object);

	[TestMethod]
	public void AddEntryShouldMarkDirtyAndSelectEntry()
	{
		_sut.AddEntry();

		Assert.IsTrue(_sut.IsDirty);
		Assert.HasCount(1, _sut.Entries);
		Assert.AreSame(_sut.Entries[0], _sut.SelectedEntry);
	}

	[TestMethod]
	public async Task ChangingEntryPropertyShouldMarkDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);

		_sut.Entries[0].Title = "Changed";

		Assert.IsTrue(_sut.IsDirty);
	}

	[TestMethod]
	public async Task LoadPlaylistAsyncShouldNotMarkDirty()
	{
		await LoadAsync(new EntryModel("First", "http://first"), new EntryModel("Second", "http://second")).ConfigureAwait(false);

		Assert.IsFalse(_sut.IsDirty);
		Assert.HasCount(2, _sut.Entries);
		Assert.AreSame(_sut.Entries[0], _sut.SelectedEntry);
	}

	[TestMethod]
	public async Task NewPlaylistShouldDetachRemovedEntries()
	{
		await LoadAsync(new EntryModel("First", "http://first")).ConfigureAwait(false);
		IEntry removed = _sut.Entries[0];

		_sut.NewPlaylist();
		removed.Title = "Changed";

		Assert.IsFalse(_sut.IsDirty);
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
	public void CanSaveShouldRequireFilePath()
	{
		_sut.AddEntry();

		Assert.IsFalse(_sut.CanSave);

		_sut.FilePath = "playlist.m3u";

		Assert.IsTrue(_sut.CanSave);
	}

	private async Task LoadAsync(params EntryModel[] entries)
	{
		Mock<IPlaylist> playlistMock = new();
		playlistMock.SetupGet(x => x.Entries).Returns(entries);
		_fileServiceMock.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(playlistMock.Object);

		await _sut.LoadPlaylistAsync("playlist.m3u").ConfigureAwait(false);
	}
}