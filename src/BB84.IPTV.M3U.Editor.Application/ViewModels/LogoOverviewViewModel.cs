// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Queries;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the logo screen: which logos the catalog knows, which of them are downloaded, and
/// which one an entry of a playlist carries.
/// </summary>
/// <remarks>
/// The screen searches every logo of the catalog, not only the one that is picked per channel, so a
/// logo of another feed, format or tag can be looked at, downloaded and assigned. An assignment
/// writes the URL or the cached file into the <c>tvg-logo</c> of the entry; the playlist is written
/// to the database when it is saved.
/// </remarks>
public sealed class LogoOverviewViewModel : ViewModelBase, INavigateable, IDisposable
{
	/// <summary>
	/// The file type filter of a logo that is picked from disk.
	/// </summary>
	public const string ImageFilter = "Image Files (*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg)|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg|All Files (*.*)|*.*";

	/// <summary>
	/// The number of logos that are shown at once.
	/// </summary>
	private const int LogoPageSize = PagedQuery.MinPageSize;

	private readonly ILogoService _logoService;
	private readonly IPlaylistService _playlistService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IClipboardService _clipboardService;
	private readonly IEventService _eventService;
	private IReadOnlyDictionary<string, string> _pathsByUrl = new Dictionary<string, string>();
	private CancellationTokenSource? _loadTokenSource;
	private IPlaylist? _playlist;
	private bool _loading;
	private PlaylistItemViewModel? _selectedPlaylist;
	private LogoAssignmentViewModel? _selectedEntry;
	private LogoOptionResponse? _selectedLogo;
	private string _searchText = string.Empty;
	private LogoCacheFilter _cacheState;
	private bool _onlySelectedChannel = true;
	private bool _applyToWholeChannel;
	private int _pageNumber = 1;
	private int _totalPages;
	private int _totalCount;
	private bool _isBusy;
	private bool _isDirty;
	private string _statusMessage = string.Empty;
	private string _entriesStatusMessage = string.Empty;
	private AsyncActionCommand? _searchCommand;
	private AsyncActionCommand? _previousPageCommand;
	private AsyncActionCommand? _nextPageCommand;
	private AsyncActionCommand? _reloadPlaylistsCommand;
	private AsyncActionCommand? _downloadLogoCommand;
	private AsyncActionCommand? _browseFileCommand;
	private AsyncActionCommand? _copyUrlCommand;
	private AsyncActionCommand? _copyLocalPathCommand;
	private AsyncActionCommand? _saveCommand;
	private ActionCommand? _assignUrlCommand;
	private ActionCommand? _assignLocalPathCommand;
	private ActionCommand? _clearLogoCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="LogoOverviewViewModel"/> class.
	/// </summary>
	/// <param name="logoService">The service that searches and downloads the logos.</param>
	/// <param name="playlistService">The service that loads and saves stored playlists.</param>
	/// <param name="fileDialogService">The service that shows file dialogs.</param>
	/// <param name="clipboardService">The service that copies a path to the clipboard.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	public LogoOverviewViewModel(
		ILogoService logoService,
		IPlaylistService playlistService,
		IFileDialogService fileDialogService,
		IClipboardService clipboardService,
		IEventService eventService)
	{
		_logoService = logoService;
		_playlistService = playlistService;
		_fileDialogService = fileDialogService;
		_clipboardService = clipboardService;
		_eventService = eventService;

		// The commands depend on the selection and the rows, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Entries.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
		Logos.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the stored playlists to pick from.
	/// </summary>
	public ObservableCollection<PlaylistItemViewModel> Playlists { get; } = [];

	/// <summary>
	/// Gets the entries of the selected playlist and the logo each of them carries.
	/// </summary>
	public ObservableCollection<LogoAssignmentViewModel> Entries { get; } = [];

	/// <summary>
	/// Gets the logos of the current page.
	/// </summary>
	public ObservableCollection<LogoOptionResponse> Logos { get; } = [];

	/// <summary>
	/// Gets or sets the playlist whose entries the logos are assigned to.
	/// </summary>
	public PlaylistItemViewModel? SelectedPlaylist
	{
		get => _selectedPlaylist;
		set
		{
			// A load of its own picks the playlist up, so it is not read twice.
			if (SetProperty(ref _selectedPlaylist, value) && !_loading)
				_ = LoadEntriesAndReportAsync();
		}
	}

	/// <summary>
	/// Gets or sets the entry a logo is assigned to.
	/// </summary>
	[NotifyChanged(nameof(CanAssign), nameof(CanAssignLocalPath))]
	public LogoAssignmentViewModel? SelectedEntry
	{
		get => _selectedEntry;
		set
		{
			if (!SetProperty(ref _selectedEntry, value))
				return;

			// The logos of the channel of the entry are the ones that are looked for next.
			if (OnlySelectedChannel && !_loading)
				_ = LoadLogosAndReportAsync(1);
		}
	}

	/// <summary>
	/// Gets or sets the logo that is assigned, downloaded or copied.
	/// </summary>
	[NotifyChanged(nameof(CanAssign), nameof(CanAssignLocalPath))]
	public LogoOptionResponse? SelectedLogo
	{
		get => _selectedLogo;
		set => SetProperty(ref _selectedLogo, value);
	}

	/// <summary>
	/// Gets or sets the text a logo must hold, in its channel, name, feed, format, URL or path.
	/// </summary>
	public string SearchText
	{
		get => _searchText;
		set => SetProperty(ref _searchText, value);
	}

	/// <summary>
	/// Gets or sets which logos the search covers, by what the cache holds for them.
	/// </summary>
	public LogoCacheFilter CacheState
	{
		get => _cacheState;
		set
		{
			if (SetProperty(ref _cacheState, value))
				_ = LoadLogosAndReportAsync(1);
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the search is limited to the channel of the selected
	/// entry.
	/// </summary>
	public bool OnlySelectedChannel
	{
		get => _onlySelectedChannel;
		set
		{
			if (SetProperty(ref _onlySelectedChannel, value))
				_ = LoadLogosAndReportAsync(1);
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether an assignment covers every entry that carries the
	/// <c>tvg-id</c> of the selected one.
	/// </summary>
	/// <remarks>
	/// A playlist often holds the same channel more than once, e.g. in several groups, and those
	/// entries are meant to show the same logo.
	/// </remarks>
	public bool ApplyToWholeChannel
	{
		get => _applyToWholeChannel;
		set => SetProperty(ref _applyToWholeChannel, value);
	}

	/// <summary>
	/// Gets the number of the page that is shown, the first page is page one.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousPage), nameof(HasNextPage))]
	public int PageNumber
	{
		get => _pageNumber;
		private set => SetProperty(ref _pageNumber, value);
	}

	/// <summary>
	/// Gets the number of pages the logos of the search have.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousPage), nameof(HasNextPage))]
	public int TotalPages
	{
		get => _totalPages;
		private set => SetProperty(ref _totalPages, value);
	}

	/// <summary>
	/// Gets the number of logos the search found.
	/// </summary>
	public int TotalCount
	{
		get => _totalCount;
		private set => SetProperty(ref _totalCount, value);
	}

	/// <summary>
	/// Indicates whether a page of logos comes before the one that is shown.
	/// </summary>
	public bool HasPreviousPage
		=> PageNumber > 1;

	/// <summary>
	/// Indicates whether a page of logos comes after the one that is shown.
	/// </summary>
	public bool HasNextPage
		=> PageNumber < TotalPages;

	/// <summary>
	/// Indicates whether the view model is busy performing a long-running operation.
	/// </summary>
	public bool IsBusy
	{
		get => _isBusy;
		private set => SetProperty(ref _isBusy, value);
	}

	/// <summary>
	/// Indicates whether a logo was assigned that is not saved yet.
	/// </summary>
	[NotifyChanged(nameof(CanSave))]
	public bool IsDirty
	{
		get => _isDirty;
		private set => SetProperty(ref _isDirty, value);
	}

	/// <summary>
	/// Indicates whether a logo can be assigned to the selected entry.
	/// </summary>
	public bool CanAssign
		=> SelectedEntry is not null && SelectedLogo is not null;

	/// <summary>
	/// Indicates whether the cached file of the selected logo can be assigned, which needs the file
	/// to be there.
	/// </summary>
	public bool CanAssignLocalPath
		=> CanAssign && SelectedLogo!.IsCached;

	/// <summary>
	/// Indicates whether there are unsaved assignments.
	/// </summary>
	public bool CanSave
		=> SelectedPlaylist is not null && IsDirty;

	/// <summary>
	/// Gets what the result shows, e.g. which page of how many logos.
	/// </summary>
	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	/// <summary>
	/// Gets what the entries of the playlist show, e.g. how many of them carry a logo.
	/// </summary>
	public string EntriesStatusMessage
	{
		get => _entriesStatusMessage;
		private set => SetProperty(ref _entriesStatusMessage, value);
	}

	/// <summary>
	/// Gets the command that searches the logos, from the first page.
	/// </summary>
	public IAsyncActionCommand SearchCommand
		=> _searchCommand ??= new AsyncActionCommand(() => LoadLogosAsync(1), () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that shows the page of logos before the one that is shown.
	/// </summary>
	public IAsyncActionCommand PreviousPageCommand
		=> _previousPageCommand ??= new AsyncActionCommand(() => LoadLogosAsync(PageNumber - 1), () => !IsBusy && HasPreviousPage, OnError);

	/// <summary>
	/// Gets the command that shows the page of logos after the one that is shown.
	/// </summary>
	public IAsyncActionCommand NextPageCommand
		=> _nextPageCommand ??= new AsyncActionCommand(() => LoadLogosAsync(PageNumber + 1), () => !IsBusy && HasNextPage, OnError);

	/// <summary>
	/// Gets the command that reads the stored playlists again.
	/// </summary>
	public IAsyncActionCommand ReloadPlaylistsCommand
		=> _reloadPlaylistsCommand ??= new AsyncActionCommand(() => LoadPlaylistsAsync(), () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that downloads the selected logo, whether or not it is the one that is
	/// picked for its channel.
	/// </summary>
	public IAsyncActionCommand DownloadLogoCommand
		=> _downloadLogoCommand ??= new AsyncActionCommand(DownloadLogoAsync, () => !IsBusy && SelectedLogo is not null, OnError);

	/// <summary>
	/// Gets the command that assigns a logo file from disk, one that is not from the catalog.
	/// </summary>
	public IAsyncActionCommand BrowseFileCommand
		=> _browseFileCommand ??= new AsyncActionCommand(BrowseFileAsync, () => !IsBusy && SelectedEntry is not null, OnError);

	/// <summary>
	/// Gets the command that copies the URL of the selected logo.
	/// </summary>
	public IAsyncActionCommand CopyUrlCommand
		=> _copyUrlCommand ??= new AsyncActionCommand(() => CopyAsync(SelectedLogo?.Url), () => SelectedLogo is not null, OnError);

	/// <summary>
	/// Gets the command that copies the path of the cached file of the selected logo.
	/// </summary>
	public IAsyncActionCommand CopyLocalPathCommand
		=> _copyLocalPathCommand ??= new AsyncActionCommand(() => CopyAsync(SelectedLogo?.LocalPath), () => SelectedLogo?.LocalPath is not null, OnError);

	/// <summary>
	/// Gets the command that saves the assignments to the stored playlist.
	/// </summary>
	public IAsyncActionCommand SaveCommand
		=> _saveCommand ??= new AsyncActionCommand(() => SaveAsync(), () => !IsBusy && CanSave, OnError);

	/// <summary>
	/// Gets the command that assigns the URL of the selected logo.
	/// </summary>
	public IActionCommand AssignUrlCommand
		=> _assignUrlCommand ??= new ActionCommand(() => Assign(SelectedLogo?.Url), () => !IsBusy && CanAssign);

	/// <summary>
	/// Gets the command that assigns the cached file of the selected logo.
	/// </summary>
	public IActionCommand AssignLocalPathCommand
		=> _assignLocalPathCommand ??= new ActionCommand(() => Assign(SelectedLogo?.LocalPath), () => !IsBusy && CanAssignLocalPath);

	/// <summary>
	/// Gets the command that takes the logo off the selected entry.
	/// </summary>
	public IActionCommand ClearLogoCommand
		=> _clearLogoCommand ??= new ActionCommand(() => Assign(null), () => !IsBusy && SelectedEntry is not null);

	/// <summary>
	/// Loads the playlists and the first page of logos, and reports a failure instead of throwing.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAndReportAsync()
	{
		try
		{
			await LoadWhileSuppressingAsync(async () =>
			{
				await LoadPlaylistsAsync().ConfigureAwait(true);
				await LoadEntriesAsync().ConfigureAwait(true);
			}).ConfigureAwait(true);

			await LoadLogosAsync(1).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Loads the stored playlists, the one that was selected stays selected if it is still there.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadPlaylistsAsync(CancellationToken cancellationToken = default)
	{
		IsBusy = true;

		try
		{
			IPagedList<PlaylistSummaryResponse> playlists = await _playlistService
				.GetPlaylistsAsync(new PlaylistSearchQuery(), cancellationToken)
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
	/// Loads the entries of the selected playlist, which is what a logo is assigned to.
	/// </summary>
	/// <remarks>
	/// The playlist is kept as it was loaded, so the assignments of the screen are written to the
	/// database when it is saved.
	/// </remarks>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadEntriesAsync(CancellationToken cancellationToken = default)
	{
		SelectedEntry = null;
		ClearEntries();
		_playlist = null;
		IsDirty = false;

		if (SelectedPlaylist is not { } playlist)
		{
			EntriesStatusMessage = string.Empty;
			return;
		}

		IsBusy = true;

		try
		{
			_playlist = await _playlistService
				.LoadAsync(playlist.Id, cancellationToken)
				.ConfigureAwait(true);

			await RefreshCachedPathsAsync().ConfigureAwait(true);

			if (_playlist is not null)
			{
				foreach (IEntry entry in _playlist.Entries)
					Entries.Add(CreateRow(entry));
			}

			SelectedEntry = Entries.FirstOrDefault();
			RaiseEntriesStatusChanged();
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads one page of the logos the search finds.
	/// </summary>
	/// <remarks>
	/// A search that is still running is cancelled, so the result of the newest one is shown.
	/// </remarks>
	/// <param name="pageNumber">The page to show, the first page is page one.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadLogosAsync(int pageNumber)
	{
		CancellationTokenSource tokenSource = new();
		CancellationTokenSource? previous = Interlocked.Exchange(ref _loadTokenSource, tokenSource);

		if (previous is not null)
		{
			await previous.CancelAsync().ConfigureAwait(true);
			previous.Dispose();
		}

		IsBusy = true;

		try
		{
			IPagedList<LogoOptionResponse> logos = await _logoService
				.SearchLogosAsync(
					new LogoSearchQuery
					{
						SearchText = SearchText,
						Channel = OnlySelectedChannel ? SelectedEntry?.Channel : null,
						CacheState = CacheState,
						PageNumber = pageNumber,
						PageSize = LogoPageSize
					},
					tokenSource.Token)
				.ConfigureAwait(true);

			int? selectedId = SelectedLogo?.Id;
			Logos.Clear();

			foreach (LogoOptionResponse logo in logos)
				Logos.Add(logo);

			SelectedLogo = Logos.FirstOrDefault(logo => logo.Id == selectedId);

			PageNumber = logos.MetaData.CurrentPage;
			TotalPages = logos.MetaData.TotalPages;
			TotalCount = logos.MetaData.TotalCount;
			StatusMessage = TotalCount is 0
				? Resources.LogoSearchNoResult
				: Resources.LogoPageStatus.FormatMessage(PageNumber, TotalPages, TotalCount);
		}
		catch (OperationCanceledException)
		{
			// A newer search took over, its result is the one that counts.
		}
		finally
		{
			IsBusy = false;

			// Nothing is cancelled any more, unless a newer search already took the field over.
			if (Interlocked.CompareExchange(ref _loadTokenSource, null, tokenSource) == tokenSource)
				tokenSource.Dispose();
		}
	}

	/// <summary>
	/// Saves the assignments to the stored playlist.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task SaveAsync(CancellationToken cancellationToken = default)
	{
		if (SelectedPlaylist is not { } playlist || _playlist is null)
			return;

		IsBusy = true;

		try
		{
			bool saved = await _playlistService
				.UpdateAsync(playlist.Id, playlist.Name, _playlist, cancellationToken)
				.ConfigureAwait(true);

			if (!saved)
			{
				StatusMessage = Resources.LogoPlaylistGone;
				return;
			}

			IsDirty = false;
			StatusMessage = Resources.LogoPlaylistSaved.FormatMessage(playlist.Name);
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Releases what a search that is still running needs.
	/// </summary>
	public void Dispose()
	{
		CancellationTokenSource? tokenSource = Interlocked.Exchange(ref _loadTokenSource, null);

		tokenSource?.Dispose();
	}

	/// <summary>
	/// Writes a logo into the selected entry, and into every entry of the same <c>tvg-id</c> if that
	/// is asked for.
	/// </summary>
	/// <param name="logo">The URL or the path to assign, <see langword="null"/> clears the logo.</param>
	private void Assign(string? logo)
	{
		if (SelectedEntry is not { } entry)
			return;

		List<LogoAssignmentViewModel> rows = ApplyToWholeChannel && entry.TvgId is { Length: > 0 } tvgId
			? [.. Entries.Where(row => string.Equals(row.TvgId, tvgId, StringComparison.OrdinalIgnoreCase))]
			: [entry];

		foreach (LogoAssignmentViewModel row in rows)
			row.Logo = logo;

		StatusMessage = Resources.LogoAssigned.FormatMessage(rows.Count);
	}

	/// <summary>
	/// Downloads the selected logo and shows the page again, so the row knows its file.
	/// </summary>
	private async Task DownloadLogoAsync()
	{
		if (SelectedLogo is not { } logo)
			return;

		IsBusy = true;

		try
		{
			string? path = await _logoService
				.CacheLogoAsync(logo.Id)
				.ConfigureAwait(true);

			StatusMessage = path is null ? Resources.LogoNotDownloaded : Resources.LogoDownloaded;
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}

		await RefreshCachedPathsAsync().ConfigureAwait(true);
		await LoadLogosAsync(PageNumber).ConfigureAwait(true);
	}

	/// <summary>
	/// Assigns an image from disk, e.g. a logo of a channel the catalog does not know.
	/// </summary>
	private async Task BrowseFileAsync()
	{
		string? filePath = await _fileDialogService
			.ShowOpenFileDialogAsync(ImageFilter, Resources.LogoBrowseTitle)
			.ConfigureAwait(true);

		if (filePath is null)
			return;

		Assign(filePath);
	}

	/// <summary>
	/// Copies a path to the clipboard and reports what was copied.
	/// </summary>
	private async Task CopyAsync(string? text)
	{
		if (text is not null && await _clipboardService.SetTextAsync(text).ConfigureAwait(true))
			PublishStatus(Resources.LogoPathCopied.FormatMessage(text));
	}

	/// <summary>
	/// Reads which logos are cached, so an entry can show the file its logo stands for.
	/// </summary>
	private async Task RefreshCachedPathsAsync()
	{
		_pathsByUrl = await _logoService
			.GetPathsByUrlAsync()
			.ConfigureAwait(true) ?? new Dictionary<string, string>();

		foreach (LogoAssignmentViewModel row in Entries)
			row.LocalPath = ResolveCachedPath(row.Logo);
	}

	/// <summary>
	/// Creates the row of one entry, with the cached file its logo stands for.
	/// </summary>
	/// <remarks>
	/// The row is watched, because the logo of an entry is also edited in the table itself, which is
	/// as much an unsaved change as an assignment is.
	/// </remarks>
	private LogoAssignmentViewModel CreateRow(IEntry entry)
	{
		LogoAssignmentViewModel row = new(entry) { LocalPath = ResolveCachedPath(entry.Metadata.TvgLogo) };

		row.PropertyChanged += OnRowPropertyChanged;

		return row;
	}

	/// <summary>
	/// Drops the rows of the playlist that was open, with what watches them.
	/// </summary>
	private void ClearEntries()
	{
		// Clear raises a reset without the removed items, so detach the handlers first.
		foreach (LogoAssignmentViewModel row in Entries)
			row.PropertyChanged -= OnRowPropertyChanged;

		Entries.Clear();
	}

	/// <summary>
	/// Keeps the cached file and the counters of a row that was given another logo.
	/// </summary>
	private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is not nameof(LogoAssignmentViewModel.Logo) || sender is not LogoAssignmentViewModel row)
			return;

		row.LocalPath = ResolveCachedPath(row.Logo);
		IsDirty = true;
		RaiseEntriesStatusChanged();
	}

	/// <summary>
	/// Reads the cached file a logo value stands for: the file of its URL, or the value itself when
	/// it already is a path.
	/// </summary>
	private string? ResolveCachedPath(string? logo)
	{
		if (string.IsNullOrWhiteSpace(logo))
			return null;

		if (_pathsByUrl.TryGetValue(logo, out string? path))
			return path;

		return Uri.TryCreate(logo, UriKind.Absolute, out Uri? uri) && !uri.IsFile ? null : logo;
	}

	private async Task LoadEntriesAndReportAsync()
	{
		try
		{
			await LoadWhileSuppressingAsync(() => LoadEntriesAsync()).ConfigureAwait(true);
			await LoadLogosAsync(1).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Runs a load that picks a playlist and an entry itself, without letting those picks start a
	/// search of their own.
	/// </summary>
	/// <remarks>
	/// The search belongs to the end of the load, where it knows the entry it is limited to. Without
	/// this, one load would read the logos several times and report a failure once per read.
	/// </remarks>
	private async Task LoadWhileSuppressingAsync(Func<Task> load)
	{
		_loading = true;

		try
		{
			await load().ConfigureAwait(true);
		}
		finally
		{
			_loading = false;
		}
	}

	private async Task LoadLogosAndReportAsync(int pageNumber)
	{
		try
		{
			await LoadLogosAsync(pageNumber).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	private void RaiseEntriesStatusChanged()
		=> EntriesStatusMessage = Entries.Count is 0
			? string.Empty
			: Resources.LogoEntriesStatus.FormatMessage(Entries.Count(entry => entry.HasLogo), Entries.Count);

	private void RaiseCommandStatesChanged()
	{
		_searchCommand?.RaiseCanExecuteChanged();
		_previousPageCommand?.RaiseCanExecuteChanged();
		_nextPageCommand?.RaiseCanExecuteChanged();
		_reloadPlaylistsCommand?.RaiseCanExecuteChanged();
		_downloadLogoCommand?.RaiseCanExecuteChanged();
		_browseFileCommand?.RaiseCanExecuteChanged();
		_copyUrlCommand?.RaiseCanExecuteChanged();
		_copyLocalPathCommand?.RaiseCanExecuteChanged();
		_saveCommand?.RaiseCanExecuteChanged();
		_assignUrlCommand?.RaiseCanExecuteChanged();
		_assignLocalPathCommand?.RaiseCanExecuteChanged();
		_clearLogoCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.LogoOperationFailed, exception));
}
