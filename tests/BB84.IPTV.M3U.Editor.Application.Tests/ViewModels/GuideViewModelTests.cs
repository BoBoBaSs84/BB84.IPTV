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
	/// <summary>
	/// More guides on a site than fit on one page, so the paging has something to do.
	/// </summary>
	private const int TotalSiteGuides = 250;

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
				new GuideMappingResponse { EntryKey = "DasErste.de", Title = "Das Erste", Site = "example.com", SiteId = "100", Lang = "de", XmltvId = "DasErste.de", DisplayName = "Das Erste", Channel = "DasErste.de" },
				new GuideMappingResponse { EntryKey = "rtsp://192.168.12.1:554", Title = "Local camera" }
			]);

		_guideServiceMock.Setup(x => x.SaveMappingsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<GuideMappingResponse>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(2);
		_guideServiceMock.Setup(x => x.ExportAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<GuideMappingResponse>?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		_guideServiceMock.Setup(x => x.GetOptionsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideOptionResponse { Channel = "DasErste.de", Site = "example.com", SiteId = "100", SiteName = "Das Erste", Lang = "de" },
				new GuideOptionResponse { Channel = "DasErste.de", Site = "other.example", SiteId = "777", SiteName = "Das Erste HD", Lang = "en" }
			]);
		_guideServiceMock.Setup(x => x.GetSitesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideSiteResponse { Site = "example.com", ChannelCount = 2, GuideCount = 3 },
				new GuideSiteResponse { Site = "other.example", ChannelCount = 1, GuideCount = 1 }
			]);
		_guideServiceMock.Setup(x => x.SearchSiteChannelsAsync(It.IsAny<GuideSiteSearchRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((GuideSiteSearchRequest request, CancellationToken _) => new PagedList<GuideOptionResponse>(
				[new GuideOptionResponse { Channel = "ZDF.de", Feed = "HD", Site = request.Site, SiteId = "200", SiteName = "ZDF HD", Lang = "de" }],
				TotalSiteGuides,
				request.PageNumber,
				request.PageSize));


		_sut = new GuideViewModel(_guideServiceMock.Object, _playlistServiceMock.Object, _fileDialogServiceMock.Object, _eventServiceMock.Object);
	}

	[TestMethod]
	public async Task SelectingARowShouldLoadTheGuidesOfItsChannel()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		_sut.SelectedMapping = _sut.Mappings[0];
		await _sut.LoadOptionsAsync().ConfigureAwait(false);

		Assert.HasCount(2, _sut.Options);
		Assert.AreEqual("example.com", _sut.SelectedOption?.Site, "The guide the row holds is picked.");
		_guideServiceMock.Verify(x => x.GetOptionsAsync("DasErste.de", null, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
	}

	[TestMethod]
	public async Task SelectingARowWithoutAChannelShouldLeaveTheGuidesEmpty()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		_sut.SelectedMapping = _sut.Mappings[1];
		await _sut.LoadOptionsAsync().ConfigureAwait(false);

		Assert.IsEmpty(_sut.Options);
		Assert.IsNull(_sut.SelectedOption);
		_guideServiceMock.Verify(x => x.GetOptionsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task ApplyOptionCommandShouldOnlyFillTheSiteTheIdentifierAndTheLanguage()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.SelectedMapping = _sut.Mappings[0];
		await _sut.LoadOptionsAsync().ConfigureAwait(false);
		_sut.SelectedOption = _sut.Options[1];

		_sut.ApplyOptionCommand.Execute();

		Assert.AreEqual("other.example", _sut.Mappings[0].Site);
		Assert.AreEqual("777", _sut.Mappings[0].SiteId);
		Assert.AreEqual("en", _sut.Mappings[0].Lang);
		Assert.AreEqual("DasErste.de", _sut.Mappings[0].XmltvId, "What is written into the file stays as it is.");
		Assert.AreEqual("Das Erste", _sut.Mappings[0].DisplayName);
	}

	[TestMethod]
	public void ApplyOptionCommandShouldNeedARowAndAGuide()
		=> Assert.IsFalse(_sut.ApplyOptionCommand.CanExecute());

	[TestMethod]
	public async Task LoadSitesAsyncShouldReadTheSitesOnlyOnce()
	{
		await _sut.LoadSitesAsync().ConfigureAwait(false);
		await _sut.LoadSitesAsync().ConfigureAwait(false);

		Assert.HasCount(2, _sut.Sites);
		_guideServiceMock.Verify(x => x.GetSitesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task RefreshSitesCommandShouldReadThemAgain()
	{
		await _sut.LoadSitesAsync().ConfigureAwait(false);

		await _sut.RefreshSitesCommand.ExecuteAsync().ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.GetSitesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[TestMethod]
	public async Task SelectingASiteShouldLoadItsGuides()
	{
		await _sut.LoadSitesAsync().ConfigureAwait(false);

		_sut.SelectedSite = _sut.Sites[0];
		await _sut.LoadSiteChannelsAsync(1).ConfigureAwait(false);

		Assert.HasCount(1, _sut.SiteChannels);
		Assert.AreEqual(1, _sut.SiteChannelPageNumber);
		Assert.IsTrue(_sut.HasNextSiteChannelPage);
		Assert.IsFalse(_sut.HasPreviousSiteChannelPage);
		_guideServiceMock.Verify(x => x.SearchSiteChannelsAsync(It.Is<GuideSiteSearchRequest>(request => request.Site == "example.com"), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
	}

	[TestMethod]
	public async Task NextSiteChannelPageCommandShouldShowTheNextPage()
	{
		await _sut.LoadSitesAsync().ConfigureAwait(false);
		_sut.SelectedSite = _sut.Sites[0];
		await _sut.LoadSiteChannelsAsync(1).ConfigureAwait(false);

		await _sut.NextSiteChannelPageCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(2, _sut.SiteChannelPageNumber);
		Assert.IsTrue(_sut.HasPreviousSiteChannelPage);
	}

	[TestMethod]
	public async Task SearchSiteChannelsCommandShouldPassTheSearchText()
	{
		await _sut.LoadSitesAsync().ConfigureAwait(false);
		_sut.SelectedSite = _sut.Sites[0];
		_sut.SiteSearchText = "ZDF";

		await _sut.SearchSiteChannelsCommand.ExecuteAsync().ConfigureAwait(false);

		_guideServiceMock.Verify(x => x.SearchSiteChannelsAsync(It.Is<GuideSiteSearchRequest>(request => request.SearchText == "ZDF"), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
	}

	[TestMethod]
	public async Task ApplySiteChannelCommandShouldFillTheSelectedRow()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		await _sut.LoadSitesAsync().ConfigureAwait(false);
		_sut.SelectedMapping = _sut.Mappings[1];
		_sut.SelectedSite = _sut.Sites[0];
		await _sut.LoadSiteChannelsAsync(1).ConfigureAwait(false);
		_sut.SelectedSiteChannel = _sut.SiteChannels[0];

		_sut.ApplySiteChannelCommand.Execute();

		Assert.AreEqual("example.com", _sut.Mappings[1].Site);
		Assert.AreEqual("200", _sut.Mappings[1].SiteId);
		Assert.AreEqual("de", _sut.Mappings[1].Lang);
		Assert.IsNull(_sut.Mappings[1].XmltvId, "A row without an identifier stays incomplete until one is given.");
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
