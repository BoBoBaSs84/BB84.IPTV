// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class GuideViewModelTests
{
	private readonly Mock<IGuideService> _guideServiceMock = new();
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly Mock<IFileDialogService> _fileDialogServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly GuideViewModel _sut;

	public GuideViewModelTests()
	{
		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((PlaylistSearchRequest? request, CancellationToken _) => new List<PlaylistSummaryResponse>
			{
				new() { Id = 1, Name = "First", EntryCount = 2 },
				new() { Id = 2, Name = "Second", EntryCount = 1 }
			}.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? Parameters.MaxPageSize));

		_guideServiceMock.Setup(x => x.GetMappingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideMappingResponse { EntryKey = "DasErste.de", Title = "Das Erste", Site = "example.com", SiteId = "100", Lang = "de", XmltvId = "DasErste.de", DisplayName = "Das Erste" },
				new GuideMappingResponse { EntryKey = "rtsp://192.168.12.1:554", Title = "Local camera" }
			]);

		_guideServiceMock.Setup(x => x.SaveMappingsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<GuideMappingResponse>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(2);
		_guideServiceMock.Setup(x => x.ExportAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<GuideMappingResponse>?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		_sut = new GuideViewModel(_guideServiceMock.Object, _playlistServiceMock.Object, _fileDialogServiceMock.Object, _eventServiceMock.Object);
	}

	[TestMethod]
	public async Task LoadAsyncShouldSelectTheFirstPlaylistAndMapIt()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.HasCount(2, _sut.Playlists);
		Assert.AreEqual(1, _sut.SelectedPlaylist?.Id);
		Assert.HasCount(2, _sut.Mappings);
		Assert.AreEqual(1, _sut.CompleteCount);
		Assert.IsTrue(_sut.HasMappings);
	}

	[TestMethod]
	public async Task SelectingAnotherPlaylistShouldMapIt()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_guideServiceMock.Invocations.Clear();

		_sut.SelectedPlaylist = _sut.Playlists[1];
		await _sut.LoadMappingsAsync().ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.GetMappingsAsync(2, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
	}

	[TestMethod]
	public async Task FillingARowShouldCountItAsComplete()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.AreEqual(1, _sut.CompleteCount);

		_sut.Mappings[1].Site = "camera.example";
		_sut.Mappings[1].SiteId = "1";
		_sut.Mappings[1].XmltvId = "Camera.local";

		Assert.AreEqual(2, _sut.CompleteCount);
		Assert.IsTrue(_sut.Mappings[1].IsComplete);
	}

	[TestMethod]
	public async Task SaveCommandShouldPassWhatTheRowsHold()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.Mappings[0].SiteId = "changed";

		await _sut.SaveCommand.ExecuteAsync().ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.SaveMappingsAsync(
			1,
			It.Is<IEnumerable<GuideMappingResponse>>(mappings => mappings.Any(mapping => mapping.SiteId == "changed")),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ExportCommandShouldAskForTheFileAndWriteTheRows()
	{
		_fileDialogServiceMock.Setup(x => x.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync("/tmp/channels.xml");

		await _sut.LoadAsync().ConfigureAwait(false);
		await _sut.ExportCommand.ExecuteAsync().ConfigureAwait(false);

		_fileDialogServiceMock.Verify(x => x.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<string>(), GuideViewModel.ChannelsFileName), Times.Once);
		_guideServiceMock.Verify(x => x.ExportAsync(1, "/tmp/channels.xml", It.IsAny<IEnumerable<GuideMappingResponse>?>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ExportCommandShouldDoNothingWhenTheDialogIsCancelled()
	{
		_fileDialogServiceMock.Setup(x => x.ShowSaveFileDialogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync((string?)null);

		await _sut.LoadAsync().ConfigureAwait(false);
		await _sut.ExportCommand.ExecuteAsync().ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.ExportAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<GuideMappingResponse>?>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public void TheCommandsShouldNeedMappings()
	{
		Assert.IsFalse(_sut.SaveCommand.CanExecute());
		Assert.IsFalse(_sut.ExportCommand.CanExecute());
	}

	[TestMethod]
	public async Task LoadAndReportAsyncShouldReportAFailureInsteadOfThrowing()
	{
		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no database"));

		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<Application.Events.ErrorOccuredEvent>()), Times.Once);
		Assert.IsFalse(_sut.IsBusy);
	}
}