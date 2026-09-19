using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.Notifications.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the view model for an M3U playlist.
/// </summary>
public sealed class PlaylistViewModel : ViewModelBase, INavigateable
{
	private readonly IFileService _fileService;
	private string _filePath = string.Empty;
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
	/// <param name="fileService">The file service instance to use.</param>
	public PlaylistViewModel(IFileService fileService)
	{
		_fileService = fileService;
		Entries = [];
		Entries.CollectionChanged += (s, e) => OnEntriesCollectionChanged(e);
	}

	/// <summary>
	/// Gets or sets the file path of the playlist.
	/// </summary>
	public string FilePath
	{
		get => _filePath;
		set => SetProperty(ref _filePath, value);
	}

	/// <summary>
	/// Gets or sets the URL for the TV guide.
	/// </summary>
	public string UrlTvg
	{
		get => _urlTvg;
		set => SetProperty(ref _urlTvg, value);
	}

	/// <summary>
	/// Gets or sets the cache period in milliseconds.
	/// </summary>
	public int Cache
	{
		get => _cache;
		set => SetProperty(ref _cache, value);
	}

	/// <summary>
	/// Gets or sets the deinterlace method.
	/// </summary>
	public Deinterlace Deinterlace
	{
		get => _deinterlace;
		set => SetProperty(ref _deinterlace, value);
	}

	/// <summary>
	/// Gets or sets the refresh period in seconds.
	/// </summary>
	public int Refresh
	{
		get => _refresh;
		set => SetProperty(ref _refresh, value);
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
		private set => SetProperty(ref _isDirty, value);
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
	/// Indicates whether the playlist can be saved without prompting for a file path.
	/// </summary>
	public bool CanSave => !IsBusy && IsDirty && FilePath.IsNotNullOrWhiteSpace();

	/// <summary>
	/// Indicates whether Save As should be enabled.
	/// </summary>
	public bool CanSaveAs => !IsBusy;

	/// <summary>
	/// Loads the playlist from the specified file path.
	/// </summary>
	/// <param name="filePath">The file path to load from. If null or empty, the current FilePath will be used.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadPlaylistAsync(string? filePath = null, CancellationToken cancellationToken = default)
	{
		if (filePath.IsNotNullOrWhiteSpace())
			FilePath = filePath;

		if (FilePath.IsNullOrWhiteSpace())
			return;

		IsBusy = true;

		try
		{
			IPlaylist? playlist = await _fileService
				.LoadAsync(FilePath, cancellationToken)
				.ConfigureAwait(true);

			if (playlist is null)
				return;

			SuppressDirtyTracking(() =>
			{
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
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Clears the current playlist and starts a new one.
	/// </summary>
	public void NewPlaylist()
	{
		SuppressDirtyTracking(() =>
		{
			UrlTvg = string.Empty;
			Cache = 0;
			Deinterlace = Deinterlace.None;
			Refresh = 0;
			_additionalAttributes = null;
			FilePath = string.Empty;
			ClearEntries();
			SelectedEntry = null;
		});
		IsDirty = false;
	}

	/// <summary>
	/// Saves the playlist to disk, optionally specifying a new path.
	/// </summary>
	/// <param name="targetPath">Optional override for the file path.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<bool> SavePlaylistAsync(string? targetPath = null, CancellationToken cancellationToken = default)
	{
		string? destinationPath = targetPath.IsNullOrWhiteSpace() ? FilePath : targetPath;
		if (destinationPath.IsNullOrWhiteSpace())
			return false;

		PlaylistSnapshot snapshot = CreateSnapshot();

		IsBusy = true;
		try
		{
			await _fileService.Save(snapshot, destinationPath, cancellationToken).ConfigureAwait(true);
			FilePath = destinationPath;
			IsDirty = false;
			return true;
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Merges entries from another playlist file into the current list.
	/// </summary>
	public async Task MergePlaylistAsync(string mergeFilePath, CancellationToken cancellationToken = default)
	{
		if (mergeFilePath.IsNullOrWhiteSpace())
			return;

		IsBusy = true;
		try
		{
			IPlaylist? playlist = await _fileService
				.LoadAsync(mergeFilePath, cancellationToken)
				.ConfigureAwait(true);

			if (playlist is null)
				return;

			foreach (EntryModel entry in playlist.Entries)
				Entries.Add(new EntryModel(entry));
		}
		finally
		{
			IsBusy = false;
		}
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

	private void OnEntriesCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems is not null)
			foreach (INotifyPropertyChanged entry in e.OldItems.OfType<INotifyPropertyChanged>())
				entry.PropertyChanged -= OnEntryPropertyChanged;

		if (e.NewItems is not null)
			foreach (INotifyPropertyChanged entry in e.NewItems.OfType<INotifyPropertyChanged>())
				entry.PropertyChanged += OnEntryPropertyChanged;

		MarkDirty();
	}

	private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
		=> MarkDirty();

	private void ClearEntries()
	{
		// Clear raises a reset without the removed items, so detach the handlers first.
		foreach (INotifyPropertyChanged entry in Entries.OfType<INotifyPropertyChanged>())
			entry.PropertyChanged -= OnEntryPropertyChanged;

		Entries.Clear();
	}

	private void MarkDirty()
	{
		if (!IsBusy && !_suppressDirtyTracking)
			IsDirty = true;
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

	private PlaylistSnapshot CreateSnapshot()
	{
		List<EntryModel> snapshotEntries = Entries
			.Select(entry => entry is EntryModel model ? new EntryModel(model) : new EntryModel(entry))
			.ToList();

		return new PlaylistSnapshot
		{
			UrlTvg = UrlTvg.IsNullOrWhiteSpace() ? null : UrlTvg,
			Cache = Cache,
			Deinterlace = Deinterlace,
			Refresh = Refresh,
			AdditionalAttributes = _additionalAttributes,
			Entries = snapshotEntries
		};
	}

	private sealed class PlaylistSnapshot : IPlaylist
	{
		public string? UrlTvg { get; set; }
		public int Cache { get; set; }
		public Deinterlace Deinterlace { get; set; }
		public int Refresh { get; set; }
		public string? AdditionalAttributes { get; set; }
		public IEnumerable<EntryModel> Entries { get; set; } = [];
	}
}
