// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class MergeViewModelTests
{
	private readonly Mock<IMergeService> _mergeServiceMock = new();
	private readonly Mock<IPlaylistService> _playlistServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly List<PlaylistSummaryResponse> _storedPlaylists =
	[
		new PlaylistSummaryResponse { Id = 1, Name = "First", EntryCount = 2 },
		new PlaylistSummaryResponse { Id = 2, Name = "Second", EntryCount = 1 },
		new PlaylistSummaryResponse { Id = 3, Name = "Third", EntryCount = 0 }
	];

	private readonly MergeViewModel _sut;

	public MergeViewModelTests()
	{
		_sut = new MergeViewModel(_mergeServiceMock.Object, _playlistServiceMock.Object, _eventServiceMock.Object);

		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((PlaylistSearchRequest? request, CancellationToken _)
				=> _storedPlaylists.ToPagedList(request?.PageNumber ?? 1, request?.PageSize ?? Parameters.MaxPageSize));

		_mergeServiceMock.Setup(x => x.PreviewAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(CreatePreview);
		_mergeServiceMock.Setup(x => x.MergeAsync(It.IsAny<MergeRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(42);
	}

	[TestMethod]
	public async Task LoadAsyncShouldFillTheSources()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.HasCount(3, _sut.Sources);
		Assert.AreEqual("First", _sut.Sources[0].Name);
		Assert.AreEqual(0, _sut.SelectedCount);
		Assert.IsFalse(_sut.HasPreview);
	}

	[TestMethod]
	public async Task LoadAndReportAsyncShouldReportAFailureInsteadOfThrowing()
	{
		_playlistServiceMock.Setup(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no database"));

		await _sut.LoadAndReportAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
		Assert.IsFalse(_sut.IsBusy);
	}

	[TestMethod]
	public async Task LoadAsyncShouldKeepTheSelection()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.Sources[1].IsSelected = true;

		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.IsTrue(_sut.Sources[1].IsSelected);
		Assert.AreEqual(1, _sut.SelectedCount);
	}

	[TestMethod]
	public async Task MergeShouldNeedTwoPlaylists()
	{
		await _sut.LoadAsync().ConfigureAwait(false);

		Assert.IsFalse(_sut.IsValid);
		Assert.IsFalse(_sut.PreviewCommand.CanExecute());
		Assert.IsFalse(_sut.MergeCommand.CanExecute());

		_sut.Sources[0].IsSelected = true;
		_sut.Sources[1].IsSelected = true;

		Assert.IsTrue(_sut.IsValid);
		Assert.IsTrue(_sut.PreviewCommand.CanExecute());
		Assert.IsTrue(_sut.MergeCommand.CanExecute());
	}

	[TestMethod]
	public async Task MergeShouldNeedAName()
	{
		await SelectTwoAsync().ConfigureAwait(false);

		_sut.Name = "   ";

		Assert.IsFalse(_sut.IsValid);
		Assert.IsFalse(_sut.MergeCommand.CanExecute());
	}

	[TestMethod]
	public async Task PreviewCommandShouldPassTheSelectedPlaylistsInOrder()
	{
		await SelectTwoAsync().ConfigureAwait(false);
		_sut.SelectedSource = _sut.Sources[1];
		_sut.MoveUpCommand.Execute();
		_sut.DuplicateMode = MergeDuplicateMode.ByTvgIdOrUrl;
		_sut.DuplicateResolution = MergeDuplicateResolution.KeepLast;

		await _sut.PreviewCommand.ExecuteAsync().ConfigureAwait(false);

		_mergeServiceMock.Verify(x => x.PreviewAsync(
			It.Is<MergeRequest>(request
				=> request.PlaylistIds.SequenceEqual(new[] { 2, 1 })
				&& request.DuplicateMode == MergeDuplicateMode.ByTvgIdOrUrl
				&& request.DuplicateResolution == MergeDuplicateResolution.KeepLast),
			It.IsAny<CancellationToken>()), Times.Once);
		Assert.IsTrue(_sut.HasPreview);
		Assert.HasCount(2, _sut.PreviewEntries);
		Assert.HasCount(2, _sut.GroupMappings);
	}

	[TestMethod]
	public async Task PreviewCommandShouldKeepTheGroupRenamingsOfTheUser()
	{
		await SelectTwoAsync().ConfigureAwait(false);
		await _sut.PreviewCommand.ExecuteAsync().ConfigureAwait(false);
		_sut.GroupMappings[0].TargetGroup = "Nachrichten";

		await _sut.PreviewCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual("Nachrichten", _sut.GroupMappings[0].TargetGroup);
		_mergeServiceMock.Verify(x => x.PreviewAsync(
			It.Is<MergeRequest>(request => request.GroupMappings!.ContainsKey("News") && request.GroupMappings["News"] == "Nachrichten"),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task MoveCommandsShouldChangeTheOrderOfTheSources()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.SelectedSource = _sut.Sources[0];

		Assert.IsFalse(_sut.MoveUpCommand.CanExecute());

		_sut.MoveDownCommand.Execute();

		Assert.AreEqual("First", _sut.Sources[1].Name);
		Assert.AreEqual("Second", _sut.Sources[0].Name);
		Assert.AreSame(_sut.Sources[1], _sut.SelectedSource);
	}

	[TestMethod]
	public async Task MergeCommandShouldStoreTheResultAndReloadTheSources()
	{
		await SelectTwoAsync().ConfigureAwait(false);
		_sut.Name = "Everything";

		await _sut.MergeCommand.ExecuteAsync().ConfigureAwait(false);

		_mergeServiceMock.Verify(x => x.MergeAsync(It.Is<MergeRequest>(request => request.Name == "Everything"), It.IsAny<CancellationToken>()), Times.Once);
		_playlistServiceMock.Verify(x => x.GetPlaylistsAsync(It.IsAny<PlaylistSearchRequest?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	private async Task SelectTwoAsync()
	{
		await _sut.LoadAsync().ConfigureAwait(false);
		_sut.Sources[0].IsSelected = true;
		_sut.Sources[1].IsSelected = true;
	}

	private static MergePreviewResponse CreatePreview()
	{
		PlaylistModel playlist = new(new PlaylistModel(),
		[
			new EntryModel("First", "http://one", metadata: new MetadataModel { TvgId = "ARD.de", GroupTitle = "News" }),
			new EntryModel("Second", "http://two", metadata: new MetadataModel { TvgId = "ZDF.de", GroupTitle = "Public" })
		]);

		return new MergePreviewResponse
		{
			Playlist = playlist,
			SourceCount = 2,
			SourceEntryCount = 3,
			DuplicateCount = 1,
			Groups = ["News", "Public"]
		};
	}
}