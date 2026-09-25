// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Infrastructure;

/// <summary>
/// Creates the view models the views are tested with, filled with data instead of a database.
/// </summary>
internal static class ViewModelFactory
{
	/// <summary>
	/// Creates the playlist screen with two stored playlists.
	/// </summary>
	public static PlaylistsViewModel CreatePlaylists(Mock<IPlaylistService>? playlistServiceMock = null, Mock<INotificationService>? notificationServiceMock = null)
	{
		playlistServiceMock ??= new Mock<IPlaylistService>();
		notificationServiceMock ??= new Mock<INotificationService>();

		SetupPlaylistService(playlistServiceMock);

		PlaylistViewModel editor = new(playlistServiceMock.Object, new Mock<IFileService>().Object);

		return new PlaylistsViewModel(
			playlistServiceMock.Object,
			new Mock<IFileDialogService>().Object,
			notificationServiceMock.Object,
			new Mock<INavigationService>().Object,
			new Mock<IEventService>().Object,
			editor);
	}

	/// <summary>
	/// Creates the channel screen with one catalog page and one custom channel.
	/// </summary>
	public static CatalogViewModel CreateCatalog(int totalChannels = 2)
	{
		Mock<ICatalogService> catalogServiceMock = new();
		Mock<ICustomChannelService> customChannelServiceMock = new();

		catalogServiceMock.Setup(x => x.GetFiltersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new CatalogFilterResponse
		{
			Countries = [new CatalogFilterValue("DE", "Germany"), new CatalogFilterValue("FR", "France")],
			Languages = [new CatalogFilterValue("deu", "German")],
			Categories = [new CatalogFilterValue("news", "News")]
		});

		catalogServiceMock.Setup(x => x.SearchAsync(It.IsAny<CatalogSearchRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((CatalogSearchRequest request, CancellationToken _) => new PagedList<CatalogChannelResponse>(
				[
					new CatalogChannelResponse
					{
						Channel = "DasErste.de",
						Feed = "SD",
						Name = "Das Erste",
						Country = "DE",
						Languages = ["deu"],
						Categories = ["news"],
						StreamUrl = "https://example.com/ard.m3u8",
						Quality = "1080p"
					}
				],
				totalChannels,
				request.PageNumber,
				request.PageSize));

		customChannelServiceMock.Setup(x => x.GetChannelsAsync(It.IsAny<CustomChannelSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((CustomChannelSearchRequest? request, CancellationToken _) => new List<CustomChannelResponse>
			{
				new() { Id = 1, Name = "Local camera", Url = "rtsp://192.168.12.1:554", GroupTitle = "Local" }
			}.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? CatalogViewModel.CustomChannelPageSize));

		Mock<IPlaylistService> playlistServiceMock = new();
		SetupPlaylistService(playlistServiceMock);

		PlaylistViewModel editor = new(playlistServiceMock.Object, new Mock<IFileService>().Object);

		return new CatalogViewModel(
			catalogServiceMock.Object,
			customChannelServiceMock.Object,
			new Mock<INotificationService>().Object,
			new Mock<IEventService>().Object,
			editor);
	}

	/// <summary>
	/// Creates the merge screen with three stored playlists and a preview.
	/// </summary>
	public static MergeViewModel CreateMerge(Mock<IMergeService>? mergeServiceMock = null)
	{
		mergeServiceMock ??= new Mock<IMergeService>();
		Mock<IPlaylistService> playlistServiceMock = new();

		playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((PlaylistSearchRequest? request, CancellationToken _) => new List<PlaylistSummaryResponse>
			{
				new() { Id = 1, Name = "First", EntryCount = 2 },
				new() { Id = 2, Name = "Second", EntryCount = 1 },
				new() { Id = 3, Name = "Third", EntryCount = 0 }
			}.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? Parameters.MaxPageSize));

		mergeServiceMock.Setup(x => x.PreviewAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new MergePreviewResponse
			{
				Playlist = new PlaylistModel(new PlaylistModel(),
				[
					new EntryModel("Das Erste", "https://example.com/ard.m3u8", metadata: new MetadataModel { TvgId = "DasErste.de", GroupTitle = "Public" })
				]),
				SourceCount = 2,
				SourceEntryCount = 3,
				DuplicateCount = 2,
				Groups = ["Public", "Local"]
			});

		mergeServiceMock.Setup(x => x.MergeAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(9);

		return new MergeViewModel(mergeServiceMock.Object, playlistServiceMock.Object, new Mock<IEventService>().Object);
	}

	/// <summary>
	/// Creates the guide screen with two playlists and one mapped and one unmapped entry.
	/// </summary>
	public static GuideViewModel CreateGuide()
	{
		Mock<IPlaylistService> playlistServiceMock = new();
		SetupPlaylistService(playlistServiceMock);

		Mock<IGuideService> guideServiceMock = new();
		guideServiceMock.Setup(x => x.GetMappingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideMappingResponse { EntryKey = "DasErste.de", Title = "Das Erste", Site = "example.com", SiteId = "100", Lang = "de", XmltvId = "DasErste.de", DisplayName = "Das Erste", Channel = "DasErste.de" },
				new GuideMappingResponse { EntryKey = "rtsp://192.168.12.1:554", Title = "Local camera" }
			]);

		guideServiceMock.Setup(x => x.GetOptionsAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideOptionResponse { Channel = "DasErste.de", Site = "example.com", SiteId = "100", SiteName = "Das Erste", Lang = "de" },
				new GuideOptionResponse { Channel = "DasErste.de", Site = "other.example", SiteId = "777", SiteName = "Das Erste HD", Lang = "en" }
			]);

		guideServiceMock.Setup(x => x.GetSitesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			[
				new GuideSiteResponse { Site = "example.com", ChannelCount = 2, GuideCount = 3 },
				new GuideSiteResponse { Site = "other.example", ChannelCount = 1, GuideCount = 1 }
			]);

		guideServiceMock.Setup(x => x.SearchSiteChannelsAsync(It.IsAny<GuideSiteSearchRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((GuideSiteSearchRequest request, CancellationToken _) => new PagedList<GuideOptionResponse>(
				[new GuideOptionResponse { Channel = "ZDF.de", Feed = "HD", Site = request.Site, SiteId = "200", SiteName = "ZDF HD", Lang = "de" }],
				1,
				request.PageNumber,
				request.PageSize));

		return new GuideViewModel(
			guideServiceMock.Object,
			playlistServiceMock.Object,
			new Mock<IFileDialogService>().Object,
			new Mock<IEventService>().Object);
	}

	/// <summary>
	/// Creates the settings screen, with the data paths the application would work with.
	/// </summary>
	public static SettingsViewModel CreateSettings()
	{
		Mock<IPathService> pathServiceMock = new();
		pathServiceMock.SetupGet(x => x.DataDirectory).Returns(Path.Combine(Path.GetTempPath(), "bb84-iptv"));
		pathServiceMock.SetupGet(x => x.LogoDirectory).Returns(Path.Combine(Path.GetTempPath(), "bb84-iptv", "logos"));
		pathServiceMock.SetupGet(x => x.LogDirectory).Returns(Path.Combine(Path.GetTempPath(), "bb84-iptv", "logs"));
		pathServiceMock.Setup(x => x.IsValidDirectory(It.IsAny<string>())).Returns(true);

		return new SettingsViewModel(
			new Mock<ISettingsService>().Object,
			new Mock<IEventService>().Object,
			new Mock<IFileDialogService>().Object,
			pathServiceMock.Object,
			new ApplicationSettings());
	}

	/// <summary>
	/// Lets the playlist service report two stored playlists and load one with two entries.
	/// </summary>
	private static void SetupPlaylistService(Mock<IPlaylistService> playlistServiceMock)
	{
		playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((PlaylistSearchRequest? request, CancellationToken _) => new List<PlaylistSummaryResponse>
			{
				new() { Id = 1, Name = "First", EntryCount = 2 },
				new() { Id = 2, Name = "Second", EntryCount = 1 }
			}.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? PlaylistsViewModel.PageSize));

		playlistServiceMock.Setup(x => x.LoadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new PlaylistModel(new PlaylistModel { UrlTvg = "https://tvg.example", Cache = 500 },
			[
				new EntryModel("Das Erste", "https://example.com/ard.m3u8", metadata: new MetadataModel { TvgId = "DasErste.de", GroupTitle = "Public" }),
				new EntryModel("Local camera", "rtsp://192.168.12.1:554")
			]));

		playlistServiceMock.Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IPlaylist>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);
	}
}
