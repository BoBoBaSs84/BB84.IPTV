using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
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
	private bool _suppressDirtyTracking;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistViewModel"/> class.
	/// </summary>
	/// <param name="playlistService">The service that loads and saves stored playlists.</param>
	/// <param name="fileService">The file service used to read playlists to merge.</param>
	public PlaylistViewModel(IPlaylistService playlistService, IFileService fileService)
	{
		_playlistService = playlistService;
		_fileService = fileService;
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
	[NotifyChanged(nameof(SelectedEntryVisible))]
	public IEntry? SelectedEntry
	{
		get => _selectedEntry;
		set => SetProperty(ref _selectedEntry, value);
	}

	/// <summary>
	/// Gets the collection of playlist entries.
	/// </summary>
	public ObservableCollection<IEntry> Entries { get; }

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
	/// Moves the selected entry within the list.
	/// </summary>
	/// <param name="direction">-1 for up, +1 for down.</param>
	public void MoveSelectedEntry(int direction)
	{
		if (SelectedEntry is null || direction == 0)
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
		=> MarkDirty();

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