// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the guide screen: which playlist is mapped to which guide sites, and the export of
/// the <c>channels.xml</c> the iptv-org EPG grabber reads.
/// </summary>
public sealed class GuideViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// The file type filter of the <c>channels.xml</c>.
	/// </summary>
	public const string ChannelsFilter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

	/// <summary>
	/// The name the export is suggested with, as the EPG grabber expects it.
	/// </summary>
	public const string ChannelsFileName = "channels.xml";

	private readonly IGuideService _guideService;
	private readonly IPlaylistService _playlistService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IEventService _eventService;
	private PlaylistItemViewModel? _selectedPlaylist;
	private bool _isBusy;
	private string _statusMessage = string.Empty;
	private AsyncActionCommand? _saveCommand;
	private AsyncActionCommand? _exportCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuideViewModel"/> class.
	/// </summary>
	/// <param name="guideService">The service that maps the entries and writes the file.</param>
	/// <param name="playlistService">The service that stores the playlists.</param>
	/// <param name="fileDialogService">The service that shows file dialogs.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	public GuideViewModel(IGuideService guideService, IPlaylistService playlistService, IFileDialogService fileDialogService, IEventService eventService)
	{
		_guideService = guideService;
		_playlistService = playlistService;
		_fileDialogService = fileDialogService;
		_eventService = eventService;

		// The commands depend on the selection and the rows, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Mappings.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the stored playlists to pick from.
	/// </summary>
	public ObservableCollection<PlaylistItemViewModel> Playlists { get; } = [];

	/// <summary>
	/// Gets the entries of the selected playlist and what they become in the <c>channels.xml</c>.
	/// </summary>
	public ObservableCollection<GuideMappingViewModel> Mappings { get; } = [];

	/// <summary>
	/// Gets or sets the playlist whose entries are mapped.
	/// </summary>
	public PlaylistItemViewModel? SelectedPlaylist
	{
		get => _selectedPlaylist;
		set
		{
			if (SetProperty(ref _selectedPlaylist, value))
				_ = LoadMappingsAndReportAsync();
		}
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
	/// Gets what the screen is doing, e.g. how many entries can be grabbed.
	/// </summary>
	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	/// <summary>
	/// Gets the number of entries that hold everything a <c>channels.xml</c> entry needs.
	/// </summary>
	public int CompleteCount
		=> Mappings.Count(mapping => mapping.IsComplete);

	/// <summary>
	/// Indicates whether a playlist with mapped entries is selected.
	/// </summary>
	public bool HasMappings
		=> Mappings.Count > 0;

	/// <summary>
	/// Gets the command that stores the mappings of the selected playlist.
	/// </summary>
	public IAsyncActionCommand SaveCommand
		=> _saveCommand ??= new AsyncActionCommand(SaveAsync, () => !IsBusy && HasMappings, OnError);

	/// <summary>
	/// Gets the command that writes the <c>channels.xml</c>.
	/// </summary>
	public IAsyncActionCommand ExportCommand
		=> _exportCommand ??= new AsyncActionCommand(ExportAsync, () => !IsBusy && HasMappings, OnError);

	/// <summary>
	/// Loads the stored playlists, the selected one stays selected.
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

			int? selectedId = SelectedPlaylist?.Id;
			Playlists.Clear();
			foreach (PlaylistSummaryResponse playlist in playlists)
				Playlists.Add(new PlaylistItemViewModel(playlist));

			SelectedPlaylist = Playlists.FirstOrDefault(playlist => playlist.Id == selectedId)
				?? Playlists.FirstOrDefault();
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads like <see cref="LoadAsync"/> and reports a failure instead of throwing.
	/// </summary>
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
	/// Loads the mappings of the selected playlist, prefilled from the imported guides.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadMappingsAsync(CancellationToken cancellationToken = default)
	{
		Mappings.Clear();

		if (SelectedPlaylist is not { } playlist)
		{
			StatusMessage = string.Empty;
			return;
		}

		IsBusy = true;

		try
		{
			IReadOnlyList<GuideMappingResponse> mappings = await _guideService
				.GetMappingsAsync(playlist.Id, cancellationToken)
				.ConfigureAwait(true);

			foreach (GuideMappingResponse mapping in mappings)
			{
				GuideMappingViewModel row = new(mapping);
				row.PropertyChanged += OnMappingPropertyChanged;
				Mappings.Add(row);
			}

			RaiseCountsChanged();
			StatusMessage = Resources.GuideMappingStatus.FormatMessage(CompleteCount, Mappings.Count);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task LoadMappingsAndReportAsync()
	{
		try
		{
			await LoadMappingsAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	private async Task SaveAsync()
	{
		if (SelectedPlaylist is not { } playlist)
			return;

		IsBusy = true;

		try
		{
			int saved = await _guideService
				.SaveMappingsAsync(playlist.Id, Mappings.Select(mapping => mapping.ToResponse()))
				.ConfigureAwait(true);

			StatusMessage = Resources.GuideMappingsSaved.FormatMessage(saved);
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task ExportAsync()
	{
		if (SelectedPlaylist is not { } playlist)
			return;

		string? filePath = await _fileDialogService
			.ShowSaveFileDialogAsync(ChannelsFilter, Resources.GuideExportTitle, ChannelsFileName)
			.ConfigureAwait(true);

		if (filePath is null)
			return;

		IsBusy = true;

		try
		{
			int written = await _guideService
				.ExportAsync(playlist.Id, filePath, Mappings.Select(mapping => mapping.ToResponse()))
				.ConfigureAwait(true);

			StatusMessage = Resources.GuideExported.FormatMessage(written, filePath);
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private void OnMappingPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(GuideMappingViewModel.IsComplete))
			RaiseCountsChanged();
	}

	private void RaiseCountsChanged()
	{
		RaisePropertyChanged(nameof(CompleteCount));
		RaisePropertyChanged(nameof(HasMappings));
	}

	private void RaiseCommandStatesChanged()
	{
		_saveCommand?.RaiseCanExecuteChanged();
		_exportCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.GuideOperationFailed, exception));
}