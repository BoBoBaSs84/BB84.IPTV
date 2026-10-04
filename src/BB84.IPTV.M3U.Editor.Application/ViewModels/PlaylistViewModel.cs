// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.Notifications.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the editor of one stored playlist: its name, header and entries.
/// </summary>
public sealed class PlaylistViewModel : ViewModelBase
{
	private readonly IPlaylistService _playlistService;
	private readonly IFileService _fileService;
	private readonly ILogoService _logoService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IClipboardService _clipboardService;
	private readonly IEventService _eventService;
	private IReadOnlyDictionary<string, string> _pathsByUrl = new Dictionary<string, string>();
	private int? _playlistId;
	private string _name = string.Empty;
	private string _urlTvg = string.Empty;
	private int _cache;
	private Deinterlace _deinterlace;
	private int _refresh;
	private string? _additionalAttributes;
	private IEntry? _selectedEntry;
	private bool _isDirty;
	private bool _isBusy;
	private bool _isSorted;
	private bool _suppressDirtyTracking;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistViewModel"/> class.
	/// </summary>
	/// <param name="playlistService">The service that loads and saves stored playlists.</param>
	/// <param name="fileService">The file service used to read playlists to merge.</param>
	/// <param name="logoService">The service that knows which logos are cached.</param>
	/// <param name="fileDialogService">The service that shows file dialogs.</param>
	/// <param name="clipboardService">The service that copies a path to the clipboard.</param>
	/// <param name="eventService">The service that publishes error events.</param>
	public PlaylistViewModel(
		IPlaylistService playlistService,
		IFileService fileService,
		ILogoService logoService,
		IFileDialogService fileDialogService,
		IClipboardService clipboardService,
		IEventService eventService)
	{
		_playlistService = playlistService;
		_fileService = fileService;
		_logoService = logoService;
		_fileDialogService = fileDialogService;
		_clipboardService = clipboardService;
		_eventService = eventService;
		Entries = [];
		Entries.CollectionChanged += (s, e) => OnEntriesCollectionChanged(e);
	}

	/// <summary>
	/// Gets the identifier of the stored playlist being edited, <see langword="null"/> if none is open.
	/// </summary>
	[NotifyChanged(nameof(HasPlaylist))]
	public int? PlaylistId
	{
		get => _playlistId;
		private set => SetProperty(ref _playlistId, value);
	}

	/// <summary>
	/// Indicates whether a stored playlist is open in the editor.
	/// </summary>
	public bool HasPlaylist
		=> PlaylistId.HasValue;

	/// <summary>
	/// Gets or sets the name of the playlist.
	/// </summary>
	public string Name
	{
		get => _name;
		set => SetPropertyAndMarkDirty(ref _name, value);
	}

	/// <summary>
	/// Gets or sets the URL for the TV guide.
	/// </summary>
	public string UrlTvg
	{
		get => _urlTvg;
		set => SetPropertyAndMarkDirty(ref _urlTvg, value);
	}

	/// <summary>
	/// Gets or sets the cache period in milliseconds.
	/// </summary>
	public int Cache
	{
		get => _cache;
		set => SetPropertyAndMarkDirty(ref _cache, value);
	}

	/// <summary>
	/// Gets or sets the deinterlace method.
	/// </summary>
	public Deinterlace Deinterlace
	{
		get => _deinterlace;
		set => SetPropertyAndMarkDirty(ref _deinterlace, value);
	}

	/// <summary>
	/// Gets or sets the refresh period in seconds.
	/// </summary>
	public int Refresh
	{
		get => _refresh;
		set => SetPropertyAndMarkDirty(ref _refresh, value);
	}

	/// <summary>
	/// Indicates whether the selected entry is visible.
	/// </summary>
	public bool SelectedEntryVisible
		=> SelectedEntry is not null;

	/// <summary>
	/// Gets or sets the currently selected playlist entry.
	/// </summary>
	[NotifyChanged(nameof(SelectedEntryVisible), nameof(SelectedEntryLogoPath))]
	public IEntry? SelectedEntry
	{
		get => _selectedEntry;
		set => SetProperty(ref _selectedEntry, value);
	}

	/// <summary>
	/// Gets the cached file the logo of the selected entry stands for, <see langword="null"/> if the
	/// entry has no logo or the logo is neither cached nor a path.
	/// </summary>
	/// <remarks>
	/// The <c>tvg-logo</c> of an entry is usually the URL the catalog knows, and the file it was
	/// downloaded to is what a player on this machine reads without the network.
	/// </remarks>
	public string? SelectedEntryLogoPath
		=> ResolveCachedPath(SelectedEntry?.Metadata.TvgLogo);

	/// <summary>
	/// Gets the collection of playlist entries.
	/// </summary>
	public ObservableCollection<IEntry> Entries { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the view shows the entries in a sort order of its own.
	/// </summary>
	/// <remarks>
	/// Sorting a view does not change the order of <see cref="Entries"/>, which is the order the
	/// playlist is written in, so the entries are not reordered while a sort is applied.
	/// </remarks>
	[NotifyChanged(nameof(CanReorderEntries))]
	public bool IsSorted
	{
		get => _isSorted;
		set => SetProperty(ref _isSorted, value);
	}

	/// <summary>
	/// Indicates whether the entries can be reordered, which needs the row order to be the playlist order.
	/// </summary>
	public bool CanReorderEntries
		=> !IsSorted;

	/// <summary>
	/// Indicates whether the playlist has unsaved changes.
	/// </summary>
	public bool IsDirty
	{
		get => _isDirty;
		private set
		{
			if (SetProperty(ref _isDirty, value))
				RaisePropertyChanged(nameof(CanSave));
		}
	}

	/// <summary>
	/// Indicates whether the view model is busy performing a long-running operation.
	/// </summary>
	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (SetProperty(ref _isBusy, value))
				RaisePropertyChanged(nameof(CanSave));
		}
	}

	/// <summary>
	/// Gets the reason why the playlist cannot be saved, <see langword="null"/> if it is valid.
	/// </summary>
	public string? ValidationMessage
	{
		get
		{
			if (Name.IsNullOrWhiteSpace())
				return Resources.PlaylistNameRequired;

			int entriesWithoutUrl = Entries.Count(entry => entry.FilePath.IsNullOrWhiteSpace());
			return entriesWithoutUrl > 0
				? Resources.PlaylistEntriesWithoutUrl.FormatMessage(entriesWithoutUrl)
				: null;
		}
	}

	/// <summary>
	/// Indicates whether the playlist can be saved as it is.
	/// </summary>
	public bool IsValid
		=> ValidationMessage is null;

	/// <summary>
	/// Indicates whether there are valid, unsaved changes to a stored playlist.
	/// </summary>
	public bool CanSave
		=> HasPlaylist && !IsBusy && IsDirty && IsValid;

	/// <summary>
	/// Loads a stored playlist into the editor.
	/// </summary>
	/// <param name="id">The identifier of the stored playlist.</param>
	/// <param name="name">The name of the playlist.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns><see langword="true"/> if the playlist was loaded; <see langword="false"/> if it does not exist.</returns>
	public async Task<bool> LoadAsync(int id, string name, CancellationToken cancellationToken = default)
	{
		IsBusy = true;

		try
		{
			IPlaylist? playlist = await _playlistService
				.LoadAsync(id, cancellationToken)
				.ConfigureAwait(true);

			if (playlist is null)
			{
				Clear();
				return false;
			}

			SuppressDirtyTracking(() =>
			{
				PlaylistId = id;
				Name = name;
				UrlTvg = playlist.UrlTvg ?? string.Empty;
				Cache = playlist.Cache;
				Deinterlace = playlist.Deinterlace;
				Refresh = playlist.Refresh;
				_additionalAttributes = playlist.AdditionalAttributes;

				ClearEntries();
				foreach (EntryModel entry in playlist.Entries)
					Entries.Add(new EntryModel(entry));
				SelectedEntry = Entries.FirstOrDefault();
			});

			await RefreshCachedLogoPathsAsync(cancellationToken).ConfigureAwait(true);

			IsDirty = false;
			RaiseValidationChanged();

			return true;
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Closes the playlist, the editor is empty afterwards.
	/// </summary>
	public void Clear()
	{
		SuppressDirtyTracking(() =>
		{
			PlaylistId = null;
			Name = string.Empty;
			UrlTvg = string.Empty;
			Cache = 0;
			Deinterlace = Deinterlace.None;
			Refresh = 0;
			_additionalAttributes = null;
			ClearEntries();
			SelectedEntry = null;
		});
		IsDirty = false;
		RaiseValidationChanged();
	}

	/// <summary>
	/// Saves the name, header and entries to the stored playlist.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns><see langword="true"/> if saved; <see langword="false"/> if no playlist is open, it is invalid or no longer exists.</returns>
	public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
	{
		if (PlaylistId is not int id || !IsValid)
			return false;

		IsBusy = true;

		try
		{
			bool updated = await _playlistService
				.UpdateAsync(id, Name, CreateSnapshot(), cancellationToken)
				.ConfigureAwait(true);

			if (updated)
				IsDirty = false;

			return updated;
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Appends the entries of an M3U file to the open playlist.
	/// </summary>
	/// <param name="filePath">The path of the M3U file to merge.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The number of appended entries.</returns>
	public async Task<int> MergeFileAsync(string filePath, CancellationToken cancellationToken = default)
	{
		if (!HasPlaylist || filePath.IsNullOrWhiteSpace())
			return 0;

		IsBusy = true;
		int appended = 0;

		try
		{
			IPlaylist? playlist = await _fileService
				.LoadAsync(filePath, cancellationToken)
				.ConfigureAwait(true);

			if (playlist is null)
				return 0;

			foreach (EntryModel entry in playlist.Entries)
			{
				Entries.Add(new EntryModel(entry));
				appended++;
			}
		}
		finally
		{
			IsBusy = false;
		}

		// Changes made while busy are not tracked, so the merge marks the playlist itself.
		if (appended > 0)
			MarkDirty();

		return appended;
	}

	/// <summary>
	/// Adds a new blank entry to the playlist.
	/// </summary>
	public void AddEntry()
	{
		EntryModel entry = new("New Channel", string.Empty);
		Entries.Add(entry);
		SelectedEntry = entry;
	}

	/// <summary>
	/// Appends entries to the open playlist, e.g. channels picked from the catalog.
	/// </summary>
	/// <param name="entries">The entries to append.</param>
	/// <returns>The number of appended entries.</returns>
	public int AddEntries(IEnumerable<EntryModel> entries)
	{
		ArgumentNullException.ThrowIfNull(entries);

		if (!HasPlaylist)
			return 0;

		int appended = 0;
		foreach (EntryModel entry in entries)
		{
			Entries.Add(entry);
			SelectedEntry = entry;
			appended++;
		}

		return appended;
	}

	/// <summary>
	/// Duplicates the selected entry.
	/// </summary>
	public void DuplicateSelectedEntry()
	{
		if (SelectedEntry is null)
			return;

		EntryModel duplicate = new(SelectedEntry);
		int insertIndex = Math.Max(0, Entries.IndexOf(SelectedEntry) + 1);
		Entries.Insert(insertIndex, duplicate);
		SelectedEntry = duplicate;
	}

	/// <summary>
	/// Removes the selected entry if possible.
	/// </summary>
	public void RemoveSelectedEntry()
	{
		if (SelectedEntry is null)
			return;

		int index = Entries.IndexOf(SelectedEntry);
		if (index < 0)
			return;

		Entries.RemoveAt(index);
		SelectedEntry = Entries.Count > 0
			? Entries[Math.Min(index, Entries.Count - 1)]
			: null;
	}

	/// <summary>
	/// Moves the selected entry within the list, unless the view is sorted.
	/// </summary>
	/// <param name="direction">-1 for up, +1 for down.</param>
	public void MoveSelectedEntry(int direction)
	{
		if (SelectedEntry is null || direction == 0 || !CanReorderEntries)
			return;

		int index = Entries.IndexOf(SelectedEntry);
		if (index < 0)
			return;

		int newIndex = index + direction;
		if (newIndex < 0 || newIndex >= Entries.Count)
			return;

		// Remove and insert instead of Move, not every view (e.g. the Avalonia DataGrid) handles move notifications.
		// Removing the entry may clear the selection of a bound view, so it is selected again afterwards.
		IEntry entry = Entries[index];
		Entries.RemoveAt(index);
		Entries.Insert(newIndex, entry);
		SelectedEntry = entry;
	}

	/// <summary>
	/// Creates the playlist as it is currently edited.
	/// </summary>
	/// <returns>A copy of the header and the entries.</returns>
	public IPlaylist CreateSnapshot()
	{
		List<EntryModel> snapshotEntries = Entries
			.Select(entry => entry is EntryModel model ? new EntryModel(model) : new EntryModel(entry))
			.ToList();

		PlaylistModel header = new()
		{
			UrlTvg = UrlTvg.IsNullOrWhiteSpace() ? null : UrlTvg,
			Cache = Cache,
			Deinterlace = Deinterlace,
			Refresh = Refresh,
			AdditionalAttributes = _additionalAttributes
		};

		return new PlaylistModel(header, snapshotEntries);
	}

	/// <summary>
	/// Assigns an image file from disk as the logo of the selected entry.
	/// </summary>
	/// <remarks>
	/// Covers a channel the catalog knows no logo for, e.g. a stream from the local network.
	/// </remarks>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task BrowseLogoAsync()
	{
		if (SelectedEntry is not { } entry)
			return;

		try
		{
			string? filePath = await _fileDialogService
				.ShowOpenFileDialogAsync(LogoOverviewViewModel.ImageFilter, Resources.LogoBrowseTitle)
				.ConfigureAwait(true);

			if (filePath is null)
				return;

			entry.Metadata.TvgLogo = filePath;
			RaisePropertyChanged(nameof(SelectedEntryLogoPath));
		}
		catch (Exception exception)
		{
			_eventService.Publish(new ErrorOccuredEvent(Resources.LogoOperationFailed, exception));
		}
	}

	/// <summary>
	/// Copies the logo of the selected entry, the cached file if there is one and the value itself
	/// otherwise.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task CopyLogoPathAsync()
	{
		string? path = SelectedEntryLogoPath ?? SelectedEntry?.Metadata.TvgLogo;

		if (path is null)
			return;

		try
		{
			_ = await _clipboardService
				.SetTextAsync(path)
				.ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			_eventService.Publish(new ErrorOccuredEvent(Resources.LogoOperationFailed, exception));
		}
	}

	/// <summary>
	/// Reads which logos are cached, so an entry can show the file its logo stands for.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task RefreshCachedLogoPathsAsync(CancellationToken cancellationToken)
	{
		_pathsByUrl = await _logoService
			.GetPathsByUrlAsync(cancellationToken)
			.ConfigureAwait(true) ?? new Dictionary<string, string>();

		RaisePropertyChanged(nameof(SelectedEntryLogoPath));
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

	private void SetPropertyAndMarkDirty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
	{
		if (SetProperty(ref field, value, propertyName))
			MarkDirty();
	}

	private void OnEntriesCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems is not null)
			foreach (IEntry entry in e.OldItems.OfType<IEntry>())
				Detach(entry);

		if (e.NewItems is not null)
			foreach (IEntry entry in e.NewItems.OfType<IEntry>())
				Attach(entry);

		MarkDirty();
	}

	private void Attach(IEntry entry)
	{
		if (entry is INotifyPropertyChanged notifyingEntry)
			notifyingEntry.PropertyChanged += OnEntryPropertyChanged;
		if (entry.Metadata is INotifyPropertyChanged notifyingMetadata)
			notifyingMetadata.PropertyChanged += OnEntryPropertyChanged;
	}

	private void Detach(IEntry entry)
	{
		if (entry is INotifyPropertyChanged notifyingEntry)
			notifyingEntry.PropertyChanged -= OnEntryPropertyChanged;
		if (entry.Metadata is INotifyPropertyChanged notifyingMetadata)
			notifyingMetadata.PropertyChanged -= OnEntryPropertyChanged;
	}

	private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(IMetadata.TvgLogo))
			RaisePropertyChanged(nameof(SelectedEntryLogoPath));

		MarkDirty();
	}

	private void ClearEntries()
	{
		// Clear raises a reset without the removed items, so detach the handlers first.
		foreach (IEntry entry in Entries)
			Detach(entry);

		Entries.Clear();
	}

	private void MarkDirty()
	{
		if (!IsBusy && !_suppressDirtyTracking)
		{
			IsDirty = true;
			RaiseValidationChanged();
		}
	}

	private void RaiseValidationChanged()
	{
		RaisePropertyChanged(nameof(ValidationMessage));
		RaisePropertyChanged(nameof(IsValid));
		RaisePropertyChanged(nameof(CanSave));
	}

	private void SuppressDirtyTracking(Action action)
	{
		_suppressDirtyTracking = !_suppressDirtyTracking;
		try
		{
			action();
		}
		finally
		{
			_suppressDirtyTracking = !_suppressDirtyTracking;
		}
	}
}