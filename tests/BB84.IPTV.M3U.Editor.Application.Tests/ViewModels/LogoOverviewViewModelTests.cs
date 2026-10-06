// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class LogoOverviewViewModelTests : IDisposable
{
	private const int TotalLogos = 250;
	private const string CachedUrl = "https://logo.example/ard.png";
	private const string CachedPath = "/logos/DasErste.de/DasErste.de-1.png";

	private readonly Mock<ILogoService> _logoServiceMock = new();
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly Mock<IFileDialogService> _fileDialogServiceMock = new();
	private readonly Mock<IClipboardService> _clipboardServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly List<LogoSearchRequest> _requests = [];
	private readonly LogoOverviewViewModel _sut;

	public LogoOverviewViewModelTests()
	{
		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((PlaylistSearchRequest? request, CancellationToken _) => new PagedList<PlaylistSummaryResponse>(
				[new PlaylistSummaryResponse { Id = 1, Name = "Mine", EntryCount = 2 }],
				1,
				request?.PageNumber ?? 1,
				request?.PageSize ?? 100));

		_playlistServiceMock.Setup(x => x.LoadAsync(1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new PlaylistModel(new PlaylistModel(),
			[
				new EntryModel("Das Erste", "https://example.com/ard.m3u8",
					metadata: new MetadataModel { TvgId = "DasErste.de", TvgLogo = CachedUrl }),
				new EntryModel("Das Erste in another group", "https://example.com/ard.m3u8",
					metadata: new MetadataModel { TvgId = "DasErste.de" }),
				new EntryModel("Local camera", "rtsp://192.168.12.1:554")
			]));

		_playlistServiceMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		_logoServiceMock.Setup(x => x.GetPathsByUrlAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, string> { [CachedUrl] = CachedPath });

		_logoServiceMock.Setup(x => x.SearchLogosAsync(It.IsAny<LogoSearchRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((LogoSearchRequest request, CancellationToken _) =>
			{
				_requests.Add(request);

				return new PagedList<LogoOptionResponse>(
					[
						new LogoOptionResponse { Id = 1, Channel = "DasErste.de", Url = CachedUrl, LocalPath = CachedPath, IsCached = true },
						new LogoOptionResponse { Id = 2, Channel = "DasErste.de", Url = "https://logo.example/ard-dark.png" }
					],
					TotalLogos,
					request.PageNumber,
					request.PageSize);
			});

		_clipboardServiceMock.Setup(x => x.SetTextAsync(It.IsAny<string?>())).ReturnsAsync(true);

		_sut = new LogoOverviewViewModel(
			_logoServiceMock.Object,
			_playlistServiceMock.Object,
			_fileDialogServiceMock.Object,
			_clipboardServiceMock.Object,
			_eventServiceMock.Object);
	}

	/// <summary>
	/// The view model releases the source of a search that is still running when it is disposed.
	/// </summary>
	public void Dispose()
		=> _sut.Dispose();

	[TestMethod]
	public async Task LoadAndReportAsyncShouldShowThePlaylistTheEntriesAndTheFirstPage()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		Assert.HasCount(1, _sut.Playlists);
		Assert.HasCount(3, _sut.Entries);
		Assert.HasCount(2, _sut.Logos);
		Assert.AreEqual(1, _sut.PageNumber);
		Assert.AreEqual(TotalLogos, _sut.TotalCount);
		Assert.IsTrue(_sut.HasNextPage);
		Assert.IsFalse(_sut.IsDirty);
		Assert.IsFalse(_sut.IsBusy);

		// The first entry carries the URL of a cached logo, so the row knows its file.
		Assert.AreEqual(CachedPath, _sut.Entries[0].LocalPath);
		Assert.IsNull(_sut.Entries[1].LocalPath);
	}

	[TestMethod]
	public async Task TheSearchShouldBeLimitedToTheChannelOfTheSelectedEntry()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_requests.Clear();

		_sut.SelectedEntry = _sut.Entries[0];
		await _sut.LoadLogosAsync(1).ConfigureAwait(false);

		Assert.AreEqual("DasErste.de", _requests[^1].Channel);

		_sut.OnlySelectedChannel = false;
		await _sut.LoadLogosAsync(1).ConfigureAwait(false);

		Assert.IsNull(_requests[^1].Channel);
	}

	[TestMethod]
	public async Task AssigningTheUrlShouldWriteItIntoTheSelectedEntry()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[1];
		_sut.SelectedLogo = _sut.Logos[0];

		_sut.AssignUrlCommand.Execute();

		Assert.AreEqual(CachedUrl, _sut.Entries[1].Logo);
		Assert.AreEqual(CachedPath, _sut.Entries[1].LocalPath);
		Assert.IsTrue(_sut.IsDirty);
		Assert.IsTrue(_sut.CanSave);
	}

	[TestMethod]
	public async Task AssigningTheFileShouldNeedADownloadedLogo()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[1];
		_sut.SelectedLogo = _sut.Logos[1];

		Assert.IsTrue(_sut.CanAssign);
		Assert.IsFalse(_sut.CanAssignLocalPath);
		Assert.IsFalse(_sut.AssignLocalPathCommand.CanExecute());

		_sut.SelectedLogo = _sut.Logos[0];

		Assert.IsTrue(_sut.CanAssignLocalPath);
		_sut.AssignLocalPathCommand.Execute();

		Assert.AreEqual(CachedPath, _sut.Entries[1].Logo);
	}

	[TestMethod]
	public async Task AssigningShouldCoverEveryEntryOfTheSameTvgIdWhenThatIsAskedFor()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.ApplyToWholeChannel = true;
		_sut.SelectedEntry = _sut.Entries[1];
		_sut.SelectedLogo = _sut.Logos[1];

		_sut.AssignUrlCommand.Execute();

		Assert.AreEqual("https://logo.example/ard-dark.png", _sut.Entries[0].Logo);
		Assert.AreEqual("https://logo.example/ard-dark.png", _sut.Entries[1].Logo);

		// The entry without a tvg-id belongs to no channel, so it keeps what it had.
		Assert.IsNull(_sut.Entries[2].Logo);
	}

	[TestMethod]
	public async Task ClearingShouldTakeTheLogoOffTheSelectedEntry()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[0];

		_sut.ClearLogoCommand.Execute();

		Assert.IsNull(_sut.Entries[0].Logo);
		Assert.IsNull(_sut.Entries[0].LocalPath);
		Assert.IsTrue(_sut.IsDirty);
	}

	[TestMethod]
	public async Task BrowseFileCommandShouldAssignThePickedFile()
	{
		_fileDialogServiceMock.Setup(x => x.ShowOpenFileDialogAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync("/home/user/logos/camera.png");

		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[2];

		await _sut.BrowseFileCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual("/home/user/logos/camera.png", _sut.Entries[2].Logo);
		Assert.AreEqual("/home/user/logos/camera.png", _sut.Entries[2].LocalPath);
	}

	[TestMethod]
	public async Task BrowseFileCommandShouldChangeNothingWhenTheDialogIsCancelled()
	{
		_fileDialogServiceMock.Setup(x => x.ShowOpenFileDialogAsync(It.IsAny<string>(), It.IsAny<string>()))
			.ReturnsAsync((string?)null);

		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[2];

		await _sut.BrowseFileCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.IsNull(_sut.Entries[2].Logo);
		Assert.IsFalse(_sut.IsDirty);
	}

	[TestMethod]
	public async Task DownloadLogoCommandShouldCacheTheSelectedLogoAndReadThePageAgain()
	{
		_logoServiceMock.Setup(x => x.CacheLogoAsync(2, It.IsAny<CancellationToken>()))
			.ReturnsAsync("/logos/DasErste.de/DasErste.de-2.png");

		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedLogo = _sut.Logos[1];
		_requests.Clear();

		await _sut.DownloadLogoCommand.ExecuteAsync().ConfigureAwait(false);

		_logoServiceMock.Verify(x => x.CacheLogoAsync(2, It.IsAny<CancellationToken>()), Times.Once);
		Assert.HasCount(1, _requests);
	}

	[TestMethod]
	public async Task CopyCommandsShouldCopyTheTwoPathsOfTheSelectedLogo()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedLogo = _sut.Logos[0];

		await _sut.CopyUrlCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.CopyLocalPathCommand.ExecuteAsync().ConfigureAwait(false);

		_clipboardServiceMock.Verify(x => x.SetTextAsync(CachedUrl), Times.Once);
		_clipboardServiceMock.Verify(x => x.SetTextAsync(CachedPath), Times.Once);

		// The logo of the second row is not downloaded, so there is no path to copy.
		_sut.SelectedLogo = _sut.Logos[1];

		Assert.IsFalse(_sut.CopyLocalPathCommand.CanExecute());
	}

	[TestMethod]
	public async Task SaveAsyncShouldWriteTheAssignmentsToThePlaylist()
	{
		await _sut.LoadAndReportAsync().ConfigureAwait(false);
		_sut.SelectedEntry = _sut.Entries[1];
		_sut.SelectedLogo = _sut.Logos[0];
		_sut.AssignUrlCommand.Execute();

		await _sut.SaveAsync(TestContext.CancellationToken).ConfigureAwait(false);

		_playlistServiceMock.Verify(x => x.UpdateAsync(
			1,
			"Mine",
			It.Is<IPlaylist>(playlist => playlist.Entries.Count(entry => entry.Metadata.TvgLogo == CachedUrl) == 2),
			It.IsAny<CancellationToken>()), Times.Once);
		Assert.IsFalse(_sut.IsDirty);
		Assert.IsFalse(_sut.CanSave);
	}

	[TestMethod]
	public async Task AFailedSearchShouldBeReportedInsteadOfThrown()
	{
		_logoServiceMock.Setup(x => x.SearchLogosAsync(It.IsAny<LogoSearchRequest>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The database is gone."));

		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
		Assert.IsFalse(_sut.IsBusy);
	}

	public TestContext TestContext { get; set; }
}
