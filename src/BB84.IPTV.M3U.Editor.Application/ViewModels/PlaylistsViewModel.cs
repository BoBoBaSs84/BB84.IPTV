using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the playlist screen: the list of stored playlists and the editor of the open one.
/// </summary>
public sealed class PlaylistsViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// The file type filter for M3U playlists.
	/// </summary>
	public const string PlaylistFilter = "M3U Files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All Files (*.*)|*.*";

	private readonly IPlaylistService _playlistService;
	private readonly IFileDialogService _fileDialogService;
	private readonly INotificationService _notificationService;
	private readonly INavigationService _navigationService;
	private readonly IEventService _eventService;
	private PlaylistItemViewModel? _currentPlaylist;
	private AsyncActionCommand? _newCommand;
	private AsyncActionCommand? _deleteCommand;
	private AsyncActionCommand? _importCommand;
	private AsyncActionCommand? _exportCommand;
	private AsyncActionCommand? _saveCommand;
	private AsyncActionCommand? _mergeCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistsViewModel"/> class.
	/// </summary>
	/// <param name="playlistService">The service that stores the playlists.</param>
	/// <param name="fileDialogService">The service that shows file dialogs.</param>
	/// <param name="notificationService">The service that shows messages and questions.</param>
	/// <param name="navigationService">The service that shows the playlist screen.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	/// <param name="editor">The editor of the open playlist.</param>
	public PlaylistsViewModel(IPlaylistService playlistService, IFileDialogService fileDialogService, INotificationService notificationService, INavigationService navigationService, IEventService eventService, PlaylistViewModel editor)
	{
		_playlistService = playlistService;
		_fileDialogService = fileDialogService;
		_notificationService = notificationService;
		_navigationService = navigationService;
		_eventService = eventService;
		Editor = editor;

		// The commands depend on the editor state, the UI only re-queries them when told so.
		Editor.PropertyChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the editor of the open playlist.
	/// </summary>
	public PlaylistViewModel Editor { get; }

	/// <summary>
	/// Gets the stored playlists.
	/// </summary>
	public ObservableCollection<PlaylistItemViewModel> Playlists { get; } = [];

	/// <summary>
	/// Gets the playlist that is open in the editor, <see langword="null"/> if none is open.
	/// </summary>
	public PlaylistItemViewModel? CurrentPlaylist
	{
		get => _currentPlaylist;
		private set
		{
			if (SetProperty(ref _currentPlaylist, value))
				RaiseCommandStatesChanged();
		}
	}

	/// <summary>
	/// Gets the command that creates and opens a new, empty playlist.
	/// </summary>
	public IAsyncActionCommand NewCommand
		=> _newCommand ??= new AsyncActionCommand(NewAsync, () => !Editor.IsBusy, OnError);

	/// <summary>
	/// Gets the command that deletes the open playlist after a confirmation.
	/// </summary>
	public IAsyncActionCommand DeleteCommand
		=> _deleteCommand ??= new AsyncActionCommand(DeleteAsync, HasOpenPlaylist, OnError);

	/// <summary>
	/// Gets the command that imports an M3U file as a new playlist and opens it.
	/// </summary>
	public IAsyncActionCommand ImportCommand
		=> _importCommand ??= new AsyncActionCommand(ImportAsync, () => !Editor.IsBusy, OnError);

	/// <summary>
	/// Gets the command that saves pending changes and exports the open playlist to an M3U file.
	/// </summary>
	public IAsyncActionCommand ExportCommand
		=> _exportCommand ??= new AsyncActionCommand(ExportAsync, HasOpenPlaylist, OnError);

	/// <summary>
	/// Gets the command that saves the open playlist.
	/// </summary>
	public IAsyncActionCommand SaveCommand
		=> _saveCommand ??= new AsyncActionCommand(async () => await SaveAsync().ConfigureAwait(true), () => Editor.CanSave, OnError);

	/// <summary>
	/// Gets the command that appends the entries of an M3U file to the open playlist.
	/// </summary>
	public IAsyncActionCommand MergeCommand
		=> _mergeCommand ??= new AsyncActionCommand(MergeAsync, HasOpenPlaylist, OnError);

	/// <summary>
	/// Loads the list of stored playlists, the open playlist stays open.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadPlaylistsAsync(CancellationToken cancellationToken = default)
	{
		IReadOnlyList<PlaylistSummaryResponse> summaries = await _playlistService
			.GetPlaylistsAsync(cancellationToken)
			.ConfigureAwait(true);

		int? currentId = CurrentPlaylist?.Id;
		Playlists.Clear();
		foreach (PlaylistSummaryResponse summary in summaries)
			Playlists.Add(new PlaylistItemViewModel(summary));

		CurrentPlaylist = Playlists.FirstOrDefault(item => item.Id == currentId);
	}

	/// <summary>
	/// Opens a playlist in the editor, asking first what to do with unsaved changes of the open one.
	/// </summary>
	/// <param name="item">The playlist to open.</param>
	/// <returns><see langword="true"/> if the playlist is open; <see langword="false"/> if the user cancelled.</returns>
	public async Task<bool> OpenAsync(PlaylistItemViewModel item)
	{
		if (ReferenceEquals(item, CurrentPlaylist))
			return true;

		if (!await ConfirmCloseAsync().ConfigureAwait(true))
			return false;

		await OpenCoreAsync(item).ConfigureAwait(true);
		return true;
	}

	/// <summary>
	/// Asks what to do with unsaved changes before the open playlist is closed: save, discard or cancel.
	/// </summary>
	/// <returns><see langword="true"/> if the playlist may be closed; <see langword="false"/> if the user cancelled or saving failed.</returns>
	public async Task<bool> ConfirmCloseAsync()
	{
		if (!Editor.IsDirty || CurrentPlaylist is null)
			return true;

		NotificationResult result = await _notificationService
			.ShowQuestionAsync(Resources.SaveChangesQuestion.FormatMessage(Editor.Name))
			.ConfigureAwait(true);

		switch (result)
		{
			case NotificationResult.Yes:
				return await SaveAsync().ConfigureAwait(true);
			case NotificationResult.No:
				// Discard: show the stored version again, so editor and list match the database.
				await OpenCoreAsync(CurrentPlaylist).ConfigureAwait(true);
				return true;
			default:
				return false;
		}
	}

	private async Task NewAsync()
	{
		_navigationService.NavigateTo<PlaylistsViewModel>();

		if (!await ConfirmCloseAsync().ConfigureAwait(true))
			return;

		string name = CreateUniqueName(Resources.NewPlaylistName);
		int id = await _playlistService
			.CreateAsync(name, new PlaylistModel())
			.ConfigureAwait(true);

		PlaylistItemViewModel item = new(id, name, 0);
		Playlists.Add(item);
		await OpenCoreAsync(item).ConfigureAwait(true);
	}

	private async Task DeleteAsync()
	{
		if (CurrentPlaylist is not { } item)
			return;

		NotificationResult result = await _notificationService
			.ShowQuestionAsync(Resources.DeletePlaylistQuestion.FormatMessage(item.Name))
			.ConfigureAwait(true);

		if (result is not NotificationResult.Yes)
			return;

		await _playlistService.DeleteAsync(item.Id).ConfigureAwait(true);
		Playlists.Remove(item);
		Editor.Clear();
		CurrentPlaylist = null;
	}

	private async Task ImportAsync()
	{
		string? filePath = await _fileDialogService
			.ShowOpenFileDialogAsync(PlaylistFilter, Resources.ImportPlaylistTitle)
			.ConfigureAwait(true);

		if (filePath is null)
			return;

		_navigationService.NavigateTo<PlaylistsViewModel>();

		if (!await ConfirmCloseAsync().ConfigureAwait(true))
			return;

		int id = await _playlistService
			.ImportAsync(filePath)
			.ConfigureAwait(true);

		await LoadPlaylistsAsync().ConfigureAwait(true);

		if (Playlists.FirstOrDefault(item => item.Id == id) is { } imported)
		{
			await OpenCoreAsync(imported).ConfigureAwait(true);
			PublishStatus(Resources.PlaylistImported.FormatMessage(imported.Name));
		}
	}

	private async Task ExportAsync()
	{
		if (CurrentPlaylist is not { } item || !await SaveAsync().ConfigureAwait(true))
			return;

		string? filePath = await _fileDialogService
			.ShowSaveFileDialogAsync(PlaylistFilter, Resources.ExportPlaylistTitle, $"{item.Name}.m3u")
			.ConfigureAwait(true);

		if (filePath is null)
			return;

		if (await _playlistService.ExportAsync(item.Id, filePath).ConfigureAwait(true))
			PublishStatus(Resources.PlaylistExported.FormatMessage(item.Name, filePath));
	}

	private async Task MergeAsync()
	{
		string? filePath = await _fileDialogService
			.ShowOpenFileDialogAsync(PlaylistFilter, Resources.MergePlaylistTitle)
			.ConfigureAwait(true);

		if (filePath is not null)
			await Editor.MergeFileAsync(filePath).ConfigureAwait(true);
	}

	/// <summary>
	/// Saves the open playlist if it has changes; an invalid playlist is not saved and the reason is shown.
	/// </summary>
	private async Task<bool> SaveAsync()
	{
		if (!Editor.IsDirty || CurrentPlaylist is not { } item)
			return true;

		if (Editor.ValidationMessage is { } reason)
		{
			await _notificationService
				.ShowWarningAsync(Resources.PlaylistCannotBeSaved.FormatMessage(reason))
				.ConfigureAwait(true);
			return false;
		}

		if (!await Editor.SaveAsync().ConfigureAwait(true))
			return false;

		item.Update(Editor.Name.Trim(), Editor.Entries.Count);
		PublishStatus(Resources.PlaylistSaved.FormatMessage(item.Name));
		return true;
	}

	private async Task OpenCoreAsync(PlaylistItemViewModel item)
	{
		if (await Editor.LoadAsync(item.Id, item.Name).ConfigureAwait(true))
		{
			CurrentPlaylist = item;
			return;
		}

		// Deleted elsewhere in the meantime.
		Playlists.Remove(item);
		CurrentPlaylist = null;
	}

	private string CreateUniqueName(string baseName)
	{
		HashSet<string> names = new(Playlists.Select(item => item.Name), StringComparer.CurrentCultureIgnoreCase);
		string name = baseName;

		for (int number = 2; names.Contains(name); number++)
			name = $"{baseName} ({number})";

		return name;
	}

	private bool HasOpenPlaylist()
		=> CurrentPlaylist is not null && !Editor.IsBusy;

	private void RaiseCommandStatesChanged()
	{
		_newCommand?.RaiseCanExecuteChanged();
		_deleteCommand?.RaiseCanExecuteChanged();
		_importCommand?.RaiseCanExecuteChanged();
		_exportCommand?.RaiseCanExecuteChanged();
		_saveCommand?.RaiseCanExecuteChanged();
		_mergeCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.PlaylistOperationFailed, exception));
}