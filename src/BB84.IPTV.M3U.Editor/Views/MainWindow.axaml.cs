using Avalonia.Controls;
using Avalonia.Interactivity;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Views;

/// <summary>
/// The main window class.
/// </summary>
public partial class MainWindow : Window
{
	private const string PlaylistFilter = "M3U Files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All Files (*.*)|*.*";

	private readonly INavigationService _navigationService = default!;
	private readonly IFileDialogService _fileDialogService = default!;
	private readonly PlaylistViewModel _playlistViewModel = default!;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	/// <remarks>
	/// Only used by the XAML previewer, the application uses the constructor with dependencies.
	/// </remarks>
	public MainWindow()
		=> InitializeComponent();

	/// <summary>
	/// Initializes a new instance of the <see cref="MainWindow"/> class.
	/// </summary>
	/// <param name="navigationService">The navigation service instance to use.</param>
	/// <param name="fileDialogService">The file dialog service instance to use.</param>
	/// <param name="mainViewModel">The main view model instance to use.</param>
	/// <param name="playlistViewModel">The playlist view model instance to use.</param>
	public MainWindow(INavigationService navigationService, IFileDialogService fileDialogService, MainViewModel mainViewModel, PlaylistViewModel playlistViewModel) : this()
	{
		_navigationService = navigationService;
		_fileDialogService = fileDialogService;
		_playlistViewModel = playlistViewModel;

		DataContext = mainViewModel;

		_playlistViewModel.PropertyChanged += (s, e) => OnPlaylistViewModelPropertyChanged(e.PropertyName);
		UpdateFileMenuState();
	}

	private void NewMenuItem_Click(object? sender, RoutedEventArgs e)
	{
		_playlistViewModel.NewPlaylist();
		_navigationService.NavigateTo<PlaylistViewModel>();
	}

	private async void OpenMenuItem_Click(object? sender, RoutedEventArgs e)
	{
		string? filePath = await _fileDialogService
			.ShowOpenFileDialogAsync(PlaylistFilter, "Open M3U File")
			.ConfigureAwait(true);

		if (filePath is not null)
		{
			_navigationService.NavigateTo<PlaylistViewModel>();
			await _playlistViewModel.LoadPlaylistAsync(filePath).ConfigureAwait(true);
		}
	}

	private async void SaveMenuItem_Click(object? sender, RoutedEventArgs e)
	{
		if (await _playlistViewModel.SavePlaylistAsync().ConfigureAwait(true))
			return;

		await SavePlaylistAsAsync().ConfigureAwait(true);
	}

	private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
		=> await SavePlaylistAsAsync().ConfigureAwait(true);

	private async Task SavePlaylistAsAsync()
	{
		string? filePath = await _fileDialogService
			.ShowSaveFileDialogAsync(PlaylistFilter, "Save Playlist As")
			.ConfigureAwait(true);

		if (filePath is not null)
			await _playlistViewModel.SavePlaylistAsync(filePath).ConfigureAwait(true);
	}

	private async void MergeMenuItem_Click(object? sender, RoutedEventArgs e)
	{
		string? filePath = await _fileDialogService
			.ShowOpenFileDialogAsync(PlaylistFilter, "Merge Playlist")
			.ConfigureAwait(true);

		if (filePath is not null)
			await _playlistViewModel.MergePlaylistAsync(filePath).ConfigureAwait(true);
	}

	private void DatabaseMenuItem_Click(object? sender, RoutedEventArgs e)
		=> _navigationService.NavigateTo<DatabaseViewModel>();

	private void OnPlaylistViewModelPropertyChanged(string? propertyName)
	{
		if (propertyName is nameof(PlaylistViewModel.IsDirty) or nameof(PlaylistViewModel.IsBusy) or nameof(PlaylistViewModel.FilePath))
			UpdateFileMenuState();
	}

	private void UpdateFileMenuState()
	{
		SaveMenuItem.IsEnabled = _playlistViewModel.CanSave;
		SaveAsMenuItem.IsEnabled = _playlistViewModel.CanSaveAs;
	}
}