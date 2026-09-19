using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class PlaylistsViewModelTests
{
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly Mock<IFileDialogService> _fileDialogServiceMock = new();
	private readonly Mock<INotificationService> _notificationServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly PlaylistsViewModel _sut;

	public PlaylistsViewModelTests()
	{
		PlaylistViewModel editor = new(_playlistServiceMock.Object, new Mock<IFileService>().Object);
		_sut = new PlaylistsViewModel(_playlistServiceMock.Object, _fileDialogServiceMock.Object, _notificationServiceMock.Object, new Mock<INavigationService>().Object, _eventServiceMock.Object, editor);

		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
		[
			new PlaylistSummaryResponse { Id = 1, Name = "First", EntryCount = 1 },
			new PlaylistSummaryResponse { Id = 2, Name = "Second", EntryCount = 0 }
		]);
		_playlistServiceMock.Setup(x => x.LoadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new PlaylistModel(new PlaylistModel(), [new EntryModel("Entry", "http://entry")]));
		_playlistServiceMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
	}

	[TestMethod]
	public async Task LoadPlaylistsAsyncShouldKeepTheOpenPlaylist()
	{
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);
		await _sut.OpenAsync(_sut.Playlists[1]).ConfigureAwait(false);

		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);

		Assert.HasCount(2, _sut.Playlists);
		Assert.AreEqual(2, _sut.CurrentPlaylist!.Id);
		Assert.AreSame(_sut.Playlists[1], _sut.CurrentPlaylist);
	}

	[TestMethod]
	public async Task OpenAsyncShouldLoadThePlaylistIntoTheEditor()
	{
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);

		bool opened = await _sut.OpenAsync(_sut.Playlists[0]).ConfigureAwait(false);

		Assert.IsTrue(opened);
		Assert.AreSame(_sut.Playlists[0], _sut.CurrentPlaylist);
		Assert.AreEqual("First", _sut.Editor.Name);
		Assert.IsFalse(_sut.SaveCommand.CanExecute());
		Assert.IsTrue(_sut.DeleteCommand.CanExecute());
	}

	[TestMethod]
	public async Task OpenAsyncShouldSaveChangesWhenTheUserAnswersYes()
	{
		await OpenFirstWithChangesAsync(NotificationResult.Yes).ConfigureAwait(false);

		bool opened = await _sut.OpenAsync(_sut.Playlists[1]).ConfigureAwait(false);

		Assert.IsTrue(opened);
		Assert.AreEqual("Second", _sut.Editor.Name);
		Assert.AreEqual("First changed", _sut.Playlists[0].Name);
		_playlistServiceMock.Verify(x => x.UpdateAsync(1, "First changed", It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task OpenAsyncShouldDiscardChangesWhenTheUserAnswersNo()
	{
		await OpenFirstWithChangesAsync(NotificationResult.No).ConfigureAwait(false);

		bool opened = await _sut.OpenAsync(_sut.Playlists[1]).ConfigureAwait(false);

		Assert.IsTrue(opened);
		Assert.AreEqual("Second", _sut.Editor.Name);
		Assert.AreEqual("First", _sut.Playlists[0].Name);
		_playlistServiceMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task OpenAsyncShouldStayWhenTheUserClosesTheQuestion()
	{
		await OpenFirstWithChangesAsync(NotificationResult.None).ConfigureAwait(false);

		bool opened = await _sut.OpenAsync(_sut.Playlists[1]).ConfigureAwait(false);

		Assert.IsFalse(opened);
		Assert.AreSame(_sut.Playlists[0], _sut.CurrentPlaylist);
		Assert.AreEqual("First changed", _sut.Editor.Name);
		Assert.IsTrue(_sut.Editor.IsDirty);
	}

	[TestMethod]
	public async Task ConfirmCloseAsyncShouldNotSaveAnInvalidPlaylist()
	{
		await OpenFirstWithChangesAsync(NotificationResult.Yes).ConfigureAwait(false);
		_sut.Editor.Name = string.Empty;

		bool canClose = await _sut.ConfirmCloseAsync().ConfigureAwait(false);

		Assert.IsFalse(canClose);
		_notificationServiceMock.Verify(x => x.ShowWarningAsync(It.IsAny<string>()), Times.Once);
		_playlistServiceMock.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task NewCommandShouldCreateAndOpenAPlaylistWithAUniqueName()
	{
		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync([new PlaylistSummaryResponse { Id = 1, Name = "New Playlist", EntryCount = 0 }]);
		_playlistServiceMock.Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>())).ReturnsAsync(7);
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);

		await _sut.NewCommand.ExecuteAsync().ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.CreateAsync("New Playlist (2)", It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Once);
		Assert.AreEqual(7, _sut.CurrentPlaylist!.Id);
		Assert.AreEqual(7, _sut.Editor.PlaylistId);
	}

	[TestMethod]
	[DataRow(NotificationResult.Yes, true)]
	[DataRow(NotificationResult.No, false)]
	public async Task DeleteCommandShouldDeleteOnlyWhenConfirmed(NotificationResult answer, bool deleted)
	{
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);
		await _sut.OpenAsync(_sut.Playlists[0]).ConfigureAwait(false);
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(answer);

		await _sut.DeleteCommand.ExecuteAsync().ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), deleted ? Times.Once() : Times.Never());
		Assert.HasCount(deleted ? 1 : 2, _sut.Playlists);
		Assert.AreEqual(!deleted, _sut.Editor.HasPlaylist);
	}

	[TestMethod]
	public async Task ImportCommandShouldImportAndOpenThePlaylist()
	{
		_fileDialogServiceMock.Setup(x => x.ShowOpenFileDialogAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("tv.m3u");
		_playlistServiceMock.Setup(x => x.ImportAsync("tv.m3u", null, It.IsAny<CancellationToken>())).ReturnsAsync(2);

		await _sut.ImportCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(2, _sut.CurrentPlaylist!.Id);
		Assert.AreEqual("Second", _sut.Editor.Name);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<DelayedStatusChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task ExportCommandShouldSavePendingChangesFirst()
	{
		await OpenFirstWithChangesAsync(NotificationResult.None).ConfigureAwait(false);
		_fileDialogServiceMock.Setup(x => x.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<string>(), "First changed.m3u")).ReturnsAsync("out.m3u");
		_playlistServiceMock.Setup(x => x.ExportAsync(1, "out.m3u", It.IsAny<CancellationToken>())).ReturnsAsync(true);

		await _sut.ExportCommand.ExecuteAsync().ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.UpdateAsync(1, "First changed", It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()), Times.Once);
		_playlistServiceMock.Verify(x => x.ExportAsync(1, "out.m3u", It.IsAny<CancellationToken>()), Times.Once);
		Assert.IsFalse(_sut.Editor.IsDirty);
	}

	[TestMethod]
	public async Task SaveCommandShouldBeEnabledOnlyForValidChanges()
	{
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);
		await _sut.OpenAsync(_sut.Playlists[0]).ConfigureAwait(false);
		int changed = 0;
		_sut.SaveCommand.CanExecuteChanged += (s, e) => changed++;

		_sut.Editor.Name = "Renamed";

		Assert.IsTrue(_sut.SaveCommand.CanExecute());
		Assert.IsGreaterThan(0, changed);

		_sut.Editor.AddEntry();

		Assert.IsFalse(_sut.SaveCommand.CanExecute());
	}

	private async Task OpenFirstWithChangesAsync(NotificationResult answer)
	{
		await _sut.LoadPlaylistsAsync().ConfigureAwait(false);
		await _sut.OpenAsync(_sut.Playlists[0]).ConfigureAwait(false);
		_sut.Editor.Name = "First changed";
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(answer);
	}
}