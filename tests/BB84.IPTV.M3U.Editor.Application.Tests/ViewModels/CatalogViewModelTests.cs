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
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class CatalogViewModelTests
{
	/// <summary>
	/// More channels than fit on one page, so the paging commands have something to do.
	/// </summary>
	private const int TotalChannels = CatalogViewModel.PageSize + 1;

	/// <summary>
	/// The stored custom channels the service mock reads, writes and pages over.
	/// </summary>
	private readonly List<CustomChannelResponse> _storedCustomChannels =
		[new CustomChannelResponse { Id = 1, Name = "Local camera", Url = "rtsp://192.168.12.1:554" }];

	private readonly Mock<ICatalogService> _catalogServiceMock = new();
	private readonly Mock<ICustomChannelService> _customChannelServiceMock = new();
	private readonly Mock<INotificationService> _notificationServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly PlaylistViewModel _editor;
	private readonly CatalogViewModel _sut;

	public CatalogViewModelTests()
	{
		_editor = new PlaylistViewModel(_playlistServiceMock.Object, new Mock<IFileService>().Object);
		_sut = new CatalogViewModel(_catalogServiceMock.Object, _customChannelServiceMock.Object, _notificationServiceMock.Object, _eventServiceMock.Object, _editor);

		_playlistServiceMock.Setup(x => x.LoadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new PlaylistModel(new PlaylistModel(), []));

		_catalogServiceMock.Setup(x => x.GetFiltersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new CatalogFilterResponse
		{
			Countries = [new CatalogFilterValue("DE", "Germany")],
			Languages = [new CatalogFilterValue("deu", "German")],
			Categories = [new CatalogFilterValue("news", "News")]
		});
		_catalogServiceMock.Setup(x => x.SearchAsync(It.IsAny<CatalogSearchRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((CatalogSearchRequest request, CancellationToken _)
				=> new PagedList<CatalogChannelResponse>([CreateCatalogChannel()], TotalChannels, request.PageNumber, request.PageSize));
		_customChannelServiceMock.Setup(x => x.GetChannelsAsync(It.IsAny<CustomChannelSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((CustomChannelSearchRequest? request, CancellationToken _)
				=> _storedCustomChannels.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? CatalogViewModel.CustomChannelPageSize));
		_customChannelServiceMock.Setup(x => x.CreateAsync(It.IsAny<CustomChannelResponse>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((CustomChannelResponse channel, CancellationToken _) =>
			{
				CustomChannelResponse stored = new() { Id = 7, Name = channel.Name, Url = channel.Url, GroupTitle = channel.GroupTitle, TvgId = channel.TvgId, TvgLogo = channel.TvgLogo };
				_storedCustomChannels.Add(stored);
				return stored.Id;
			});
		_customChannelServiceMock.Setup(x => x.UpdateAsync(It.IsAny<CustomChannelResponse>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_customChannelServiceMock.Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((int id, CancellationToken _) => _storedCustomChannels.RemoveAll(channel => channel.Id == id) > 0);
	}

	[TestMethod]
	public async Task LoadAsyncShouldFillTheFiltersAndTheCustomChannels()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.HasCount(1, _sut.Countries);
		Assert.HasCount(1, _sut.Languages);
		Assert.HasCount(1, _sut.Categories);
		Assert.HasCount(1, _sut.CustomChannels);
		Assert.AreEqual("Local camera", _sut.SelectedCustomChannel!.Name);
	}

	[TestMethod]
	public async Task LoadAndReportAsyncShouldReportAFailureInsteadOfThrowing()
	{
		_catalogServiceMock.Setup(x => x.GetFiltersAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no database"));

		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
		Assert.IsFalse(_sut.IsBusy);
	}

	[TestMethod]
	public async Task LoadAsyncShouldReadTheFiltersOnlyOnceWhenTheyAreKnown()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		await _sut.LoadAsync().ConfigureAwait(false);

		_catalogServiceMock.Verify(x => x.GetFiltersAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task LoadAsyncShouldReadTheFiltersAgainWhileTheCatalogIsEmpty()
	{
		_catalogServiceMock.Setup(x => x.GetFiltersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new CatalogFilterResponse());

		await _sut.LoadAsync().ConfigureAwait(false);
		await _sut.LoadAsync().ConfigureAwait(false);

		_catalogServiceMock.Verify(x => x.GetFiltersAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[TestMethod]
	public async Task SearchCommandShouldPassTheFilterAndFillTheResult()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.SearchText = "Erste";
		_sut.SelectedCountry = _sut.Countries[0];
		_sut.SelectedLanguage = _sut.Languages[0];
		_sut.SelectedCategory = _sut.Categories[0];
		_sut.IncludeNsfw = true;

		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		_catalogServiceMock.Verify(x => x.SearchAsync(
			It.Is<CatalogSearchRequest>(r => r.SearchText == "Erste" && r.Country == "DE" && r.Language == "deu" && r.Category == "news" && r.IncludeNsfw),
			It.IsAny<CancellationToken>()), Times.Once);
		Assert.HasCount(1, _sut.Channels);
		Assert.AreEqual("Das Erste", _sut.SelectedChannel!.Name);
		Assert.AreEqual("DasErste.de@SD", _sut.SelectedChannel.TvgId);
	}

	[TestMethod]
	public async Task SearchCommandShouldReportThePageAndStartAtTheFirstOne()
	{
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.NextPageCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(1, _sut.PageNumber);
		Assert.AreEqual(2, _sut.TotalPages);
		Assert.AreEqual(TotalChannels, _sut.TotalCount);
		Assert.IsFalse(_sut.HasPreviousPage);
		Assert.IsTrue(_sut.HasNextPage);
	}

	[TestMethod]
	public async Task NextAndPreviousPageCommandsShouldRequestTheNeighbouringPages()
	{
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.IsFalse(_sut.PreviousPageCommand.CanExecute());

		await _sut.NextPageCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(2, _sut.PageNumber);
		Assert.IsFalse(_sut.HasNextPage);
		Assert.IsTrue(_sut.PreviousPageCommand.CanExecute());
		_catalogServiceMock.Verify(x => x.SearchAsync(It.Is<CatalogSearchRequest>(r => r.PageNumber == 2), It.IsAny<CancellationToken>()), Times.Once);

		await _sut.PreviousPageCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(1, _sut.PageNumber);
		_catalogServiceMock.Verify(x => x.SearchAsync(It.Is<CatalogSearchRequest>(r => r.PageNumber == 1), It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	[TestMethod]
	public async Task ResetFiltersCommandShouldClearTheFilterAndTheResult()
	{
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);
		_sut.SearchText = "Erste";

		_sut.ResetFiltersCommand.Execute();

		Assert.AreEqual(string.Empty, _sut.SearchText);
		Assert.IsEmpty(_sut.Channels);
		Assert.IsNull(_sut.SelectedChannel);
		Assert.AreEqual(1, _sut.PageNumber);
		Assert.AreEqual(0, _sut.TotalCount);
		Assert.IsFalse(_sut.HasNextPage);
	}

	[TestMethod]
	public async Task AddChannelCommandShouldAppendTheChannelToTheOpenPlaylist()
	{
		await OpenPlaylistAsync().ConfigureAwait(false);
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		_sut.AddChannelCommand.Execute();

		Assert.HasCount(1, _editor.Entries);
		IEntry entry = _editor.Entries[0];
		Assert.AreEqual("Das Erste", entry.Title);
		Assert.AreEqual("https://example.com/ard.m3u8", entry.FilePath);
		Assert.AreEqual("DasErste.de", entry.Channel);
		Assert.AreEqual("SD", entry.Feed);
		Assert.AreEqual("DasErste.de@SD", entry.Metadata.TvgId);
		Assert.AreEqual("news", entry.Metadata.GroupTitle);
		Assert.IsTrue(_editor.IsDirty);
	}

	[TestMethod]
	public async Task AddChannelCommandShouldBeDisabledWithoutAnOpenPlaylist()
	{
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.IsFalse(_sut.AddChannelCommand.CanExecute());
		Assert.IsFalse(_sut.AddAllChannelsCommand.CanExecute());
	}

	[TestMethod]
	public async Task AddAllChannelsCommandShouldAppendEveryFoundChannel()
	{
		await OpenPlaylistAsync().ConfigureAwait(false);
		await _sut.SearchCommand.ExecuteAsync().ConfigureAwait(false);

		_sut.AddAllChannelsCommand.Execute();

		Assert.HasCount(1, _editor.Entries);
	}

	[TestMethod]
	public async Task AddCustomChannelCommandShouldAppendTheCustomChannel()
	{
		await OpenPlaylistAsync().ConfigureAwait(false);
		await _sut.LoadAsync().ConfigureAwait(false);

		_sut.AddCustomChannelCommand.Execute();

		Assert.HasCount(1, _editor.Entries);
		Assert.AreEqual("rtsp://192.168.12.1:554", _editor.Entries[0].FilePath);
		Assert.IsNull(_editor.Entries[0].Channel);
	}

	[TestMethod]
	public void NewCustomChannelCommandShouldAddAnUnsavedChannel()
	{
		_sut.NewCustomChannelCommand.Execute();

		Assert.HasCount(1, _sut.CustomChannels);
		Assert.IsTrue(_sut.SelectedCustomChannel!.IsNew);
		Assert.IsFalse(_sut.SaveCustomChannelCommand.CanExecute());
	}

	[TestMethod]
	public async Task SaveCustomChannelCommandShouldStoreANewChannel()
	{
		_sut.NewCustomChannelCommand.Execute();
		_sut.SelectedCustomChannel!.Url = "rtsp://192.168.12.1:554";

		Assert.IsTrue(_sut.SaveCustomChannelCommand.CanExecute());
		await _sut.SaveCustomChannelCommand.ExecuteAsync().ConfigureAwait(false);

		_customChannelServiceMock.Verify(x => x.CreateAsync(It.IsAny<CustomChannelResponse>(), It.IsAny<CancellationToken>()), Times.Once);
		Assert.HasCount(2, _sut.CustomChannels);
		Assert.AreEqual(7, _sut.SelectedCustomChannel!.Id);
		Assert.IsFalse(_sut.SelectedCustomChannel.IsNew);
	}

	[TestMethod]
	public async Task SaveCustomChannelCommandShouldUpdateAStoredChannel()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.SelectedCustomChannel!.Name = "Garden camera";

		await _sut.SaveCustomChannelCommand.ExecuteAsync().ConfigureAwait(false);

		_customChannelServiceMock.Verify(x => x.UpdateAsync(It.Is<CustomChannelResponse>(c => c.Id == 1 && c.Name == "Garden camera"), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task DeleteCustomChannelCommandShouldAskBeforeDeleting()
	{
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.No);
		await _sut.LoadAsync().ConfigureAwait(false);

		await _sut.DeleteCustomChannelCommand.ExecuteAsync().ConfigureAwait(false);

		_customChannelServiceMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		Assert.HasCount(1, _sut.CustomChannels);
	}

	[TestMethod]
	public async Task DeleteCustomChannelCommandShouldRemoveTheConfirmedChannel()
	{
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.Yes);
		await _sut.LoadAsync().ConfigureAwait(false);

		await _sut.DeleteCustomChannelCommand.ExecuteAsync().ConfigureAwait(false);

		_customChannelServiceMock.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
		Assert.IsEmpty(_sut.CustomChannels);
		Assert.IsNull(_sut.SelectedCustomChannel);
	}

	[TestMethod]
	public async Task DeleteCustomChannelCommandShouldDropAnUnsavedChannelWithoutAsking()
	{
		_sut.NewCustomChannelCommand.Execute();

		await _sut.DeleteCustomChannelCommand.ExecuteAsync().ConfigureAwait(false);

		_notificationServiceMock.Verify(x => x.ShowQuestionAsync(It.IsAny<string>()), Times.Never);
		Assert.IsEmpty(_sut.CustomChannels);
	}

	private async Task OpenPlaylistAsync()
		=> _ = await _editor.LoadAsync(1, "Mine").ConfigureAwait(false);

	private static CatalogChannelResponse CreateCatalogChannel() => new()
	{
		Channel = "DasErste.de",
		Feed = "SD",
		Name = "Das Erste",
		Country = "DE",
		Languages = ["deu"],
		Categories = ["news"],
		StreamUrl = "https://example.com/ard.m3u8",
		Quality = "1080p",
		LogoUrl = "https://logo.example/ard.png"
	};
}