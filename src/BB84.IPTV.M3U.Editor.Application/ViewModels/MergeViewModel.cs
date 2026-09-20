// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the merge screen: the playlists to merge, the merge options and the preview.
/// </summary>
/// <remarks>
/// The merge writes a new playlist and leaves the sources as they are, so the preview can be
/// recreated as often as needed.
/// </remarks>
public sealed class MergeViewModel : ViewModelBase, INavigateable
{
	private readonly IMergeService _mergeService;
	private readonly IPlaylistService _playlistService;
	private readonly IEventService _eventService;
	private string _name = string.Empty;
	private MergeDuplicateMode _duplicateMode;
	private MergeDuplicateResolution _duplicateResolution;
	private bool _isBusy;
	private string _previewMessage = string.Empty;
	private MergePreviewResponse? _preview;
	private ActionCommand? _moveUpCommand;
	private ActionCommand? _moveDownCommand;
	private AsyncActionCommand? _previewCommand;
	private AsyncActionCommand? _mergeCommand;
	private MergeSourceViewModel? _selectedSource;

	/// <summary>
	/// Initializes a new instance of the <see cref="MergeViewModel"/> class.
	/// </summary>
	/// <param name="mergeService">The service that merges the playlists.</param>
	/// <param name="playlistService">The service that stores the playlists.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	public MergeViewModel(IMergeService mergeService, IPlaylistService playlistService, IEventService eventService)
	{
		_mergeService = mergeService;
		_playlistService = playlistService;
		_eventService = eventService;
		_name = Resources.MergedPlaylistName;

		// The commands depend on the selection and the preview, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Sources.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the stored playlists, in the order they are merged.
	/// </summary>
	public ObservableCollection<MergeSourceViewModel> Sources { get; } = [];

	/// <summary>
	/// Gets the group titles of the preview and the titles they are renamed to.
	/// </summary>
	public ObservableCollection<MergeGroupMappingViewModel> GroupMappings { get; } = [];

	/// <summary>
	/// Gets the entries of the preview.
	/// </summary>
	public ObservableCollection<IEntry> PreviewEntries { get; } = [];

	/// <summary>
	/// Gets or sets the name of the playlist the merge creates.
	/// </summary>
	[NotifyChanged(nameof(ValidationMessage), nameof(IsValid))]
	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}

	/// <summary>
	/// Gets or sets what makes two entries the same entry.
	/// </summary>
	public MergeDuplicateMode DuplicateMode
	{
		get => _duplicateMode;
		set => SetProperty(ref _duplicateMode, value);
	}

	/// <summary>
	/// Gets or sets which of two duplicate entries survives.
	/// </summary>
	public MergeDuplicateResolution DuplicateResolution
	{
		get => _duplicateResolution;
		set => SetProperty(ref _duplicateResolution, value);
	}

	/// <summary>
	/// Gets or sets the selected playlist, the one the order commands move.
	/// </summary>
	public MergeSourceViewModel? SelectedSource
	{
		get => _selectedSource;
		set => SetProperty(ref _selectedSource, value);
	}

	/// <summary>
	/// Indicates whether the view model is busy performing a long-running operation.
	/// </summary>
	public bool IsBusy
	{
		get => _isBusy;
		private set => SetProperty(ref _isBusy, value);
	}

	/// <summary>
	/// Gets what the preview holds, e.g. the number of entries and dropped duplicates.
	/// </summary>
	public string PreviewMessage
	{
		get => _previewMessage;
		private set => SetProperty(ref _previewMessage, value);
	}

	/// <summary>
	/// Indicates whether a preview has been created.
	/// </summary>
	[NotifyChanged(nameof(HasPreview))]
	public MergePreviewResponse? Preview
	{
		get => _preview;
		private set => SetProperty(ref _preview, value);
	}

	/// <summary>
	/// Indicates whether a preview is shown.
	/// </summary>
	public bool HasPreview
		=> Preview is not null;

	/// <summary>
	/// Gets the number of playlists that take part in the merge.
	/// </summary>
	public int SelectedCount
		=> Sources.Count(source => source.IsSelected);

	/// <summary>
	/// Gets the reason why the merge cannot run, <see langword="null"/> if it can.
	/// </summary>
	public string? ValidationMessage
	{
		get
		{
			if (SelectedCount < 2)
				return Resources.MergeNeedsTwoPlaylists;

			return string.IsNullOrWhiteSpace(Name)
				? Resources.PlaylistNameRequired
				: null;
		}
	}

	/// <summary>
	/// Indicates whether the merge can run as it is set up.
	/// </summary>
	public bool IsValid
		=> ValidationMessage is null;

	/// <summary>
	/// Gets the command that moves the selected playlist up, so it is merged earlier.
	/// </summary>
	public IActionCommand MoveUpCommand
		=> _moveUpCommand ??= new ActionCommand(() => MoveSelectedSource(-1), () => CanMoveSelectedSource(-1));

	/// <summary>
	/// Gets the command that moves the selected playlist down, so it is merged later.
	/// </summary>
	public IActionCommand MoveDownCommand
		=> _moveDownCommand ??= new ActionCommand(() => MoveSelectedSource(1), () => CanMoveSelectedSource(1));

	/// <summary>
	/// Gets the command that merges the playlists without storing the result.
	/// </summary>
	public IAsyncActionCommand PreviewCommand
		=> _previewCommand ??= new AsyncActionCommand(() => PreviewAsync(), () => !IsBusy && IsValid, OnError);

	/// <summary>
	/// Gets the command that stores the merged playlist.
	/// </summary>
	public IAsyncActionCommand MergeCommand
		=> _mergeCommand ??= new AsyncActionCommand(MergeAsync, () => !IsBusy && IsValid, OnError);

	/// <summary>
	/// Loads like <see cref="LoadAsync"/> and reports a failure instead of throwing.
	/// </summary>
	/// <remarks>
	/// For callers that cannot await, e.g. a view that is shown; a failure would be lost otherwise,
	/// leaving an empty screen without a reason.
	/// </remarks>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAndReportAsync()
	{
		try
		{
			await LoadAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Loads the stored playlists, the selection and the preview are dropped.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		IsBusy = true;

		try
		{
			IPagedList<PlaylistSummaryResponse> playlists = await _playlistService
				.GetPlaylistsAsync(new PlaylistSearchRequest(), cancellationToken)
				.ConfigureAwait(true);

			HashSet<int> selectedIds = [.. Sources.Where(source => source.IsSelected).Select(source => source.Id)];

			Detach();
			Sources.Clear();
			foreach (PlaylistSummaryResponse playlist in playlists)
			{
				MergeSourceViewModel source = new(playlist) { IsSelected = selectedIds.Contains(playlist.Id) };
				source.PropertyChanged += OnSourcePropertyChanged;
				Sources.Add(source);
			}

			SelectedSource = Sources.FirstOrDefault();
			ClearPreview();
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Merges the selected playlists without storing the result.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns><see langword="true"/> if a preview was created.</returns>
	public async Task<bool> PreviewAsync(CancellationToken cancellationToken = default)
	{
		if (!IsValid)
			return false;

		IsBusy = true;

		try
		{
			MergePreviewResponse preview = await _mergeService
				.PreviewAsync(CreateRequest(), cancellationToken)
				.ConfigureAwait(true);

			Preview = preview;

			PreviewEntries.Clear();
			foreach (EntryModel entry in preview.Playlist.Entries)
				PreviewEntries.Add(entry);

			SynchronizeGroupMappings(preview.Groups);

			PreviewMessage = Resources.MergePreviewStatus.FormatMessage(preview.SourceCount, PreviewEntries.Count, preview.DuplicateCount);

			return true;
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task MergeAsync()
	{
		if (!IsValid)
			return;

		IsBusy = true;

		try
		{
			_ = await _mergeService
				.MergeAsync(CreateRequest())
				.ConfigureAwait(true);

			PublishStatus(Resources.MergeCompleted.FormatMessage(Name.Trim()));
		}
		finally
		{
			IsBusy = false;
		}

		// The new playlist belongs in the list of sources, and its entries are the preview.
		await LoadAsync().ConfigureAwait(true);
	}

	private MergeRequest CreateRequest() => new()
	{
		PlaylistIds = [.. Sources.Where(source => source.IsSelected).Select(source => source.Id)],
		Name = Name.Trim(),
		DuplicateMode = DuplicateMode,
		DuplicateResolution = DuplicateResolution,
		GroupMappings = GroupMappings
			.Where(mapping => mapping.IsMapped)
			.ToDictionary(mapping => mapping.SourceGroup, mapping => mapping.TargetGroup, StringComparer.CurrentCultureIgnoreCase)
	};

	/// <summary>
	/// Keeps the renamings the user typed, and drops the ones whose group is gone.
	/// </summary>
	private void SynchronizeGroupMappings(IReadOnlyList<string> groups)
	{
		Dictionary<string, string> renamed = GroupMappings
			.Where(mapping => mapping.IsMapped)
			.ToDictionary(mapping => mapping.SourceGroup, mapping => mapping.TargetGroup, StringComparer.CurrentCultureIgnoreCase);

		GroupMappings.Clear();
		foreach (string group in groups)
		{
			MergeGroupMappingViewModel mapping = new(group);
			if (renamed.TryGetValue(group, out string? target))
				mapping.TargetGroup = target;

			GroupMappings.Add(mapping);
		}
	}

	private void MoveSelectedSource(int direction)
	{
		if (!CanMoveSelectedSource(direction) || SelectedSource is not { } source)
			return;

		int index = Sources.IndexOf(source);

		// Remove and insert instead of Move, not every view handles move notifications.
		Sources.RemoveAt(index);
		Sources.Insert(index + direction, source);
		SelectedSource = source;
	}

	private bool CanMoveSelectedSource(int direction)
	{
		if (IsBusy || SelectedSource is not { } source)
			return false;

		int index = Sources.IndexOf(source) + direction;

		return index >= 0 && index < Sources.Count;
	}

	private void ClearPreview()
	{
		Preview = null;
		PreviewEntries.Clear();
		GroupMappings.Clear();
		PreviewMessage = string.Empty;
	}

	private void Detach()
	{
		foreach (MergeSourceViewModel source in Sources)
			source.PropertyChanged -= OnSourcePropertyChanged;
	}

	private void OnSourcePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(MergeSourceViewModel.IsSelected))
			return;

		RaisePropertyChanged(nameof(SelectedCount));
		RaisePropertyChanged(nameof(ValidationMessage));
		RaisePropertyChanged(nameof(IsValid));
		RaiseCommandStatesChanged();
	}

	private void RaiseCommandStatesChanged()
	{
		_moveUpCommand?.RaiseCanExecuteChanged();
		_moveDownCommand?.RaiseCanExecuteChanged();
		_previewCommand?.RaiseCanExecuteChanged();
		_mergeCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.MergeOperationFailed, exception));
}