using Avalonia.Controls;
using Avalonia.Interactivity;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Views;

/// <summary>
/// The main window class.
/// </summary>
public partial class MainWindow : Window
{
	private readonly INavigationService _navigationService = default!;
	private readonly PlaylistsViewModel _playlistsViewModel = default!;
	private bool _closeConfirmed;

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
	/// <param name="mainViewModel">The main view model instance to use.</param>
	/// <param name="playlistsViewModel">The playlists view model instance to use.</param>
	public MainWindow(INavigationService navigationService, MainViewModel mainViewModel, PlaylistsViewModel playlistsViewModel) : this()
	{
		_navigationService = navigationService;
		_playlistsViewModel = playlistsViewModel;

		DataContext = mainViewModel;

		NewMenuItem.Command = playlistsViewModel.NewCommand;
		ImportMenuItem.Command = playlistsViewModel.ImportCommand;
		SaveMenuItem.Command = playlistsViewModel.SaveCommand;
		ExportMenuItem.Command = playlistsViewModel.ExportCommand;
		MergeMenuItem.Command = playlistsViewModel.MergeCommand;
	}

	/// <inheritdoc/>
	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);

		if (_closeConfirmed || e.Cancel || !_playlistsViewModel.Editor.IsDirty)
			return;

		// The question is asynchronous, so the close is cancelled now and repeated once it is answered.
		e.Cancel = true;
		_ = ConfirmCloseAsync();
	}

	private async Task ConfirmCloseAsync()
	{
		if (!await _playlistsViewModel.ConfirmCloseAsync().ConfigureAwait(true))
			return;

		_closeConfirmed = true;
		Close();
	}

	private void PlaylistsMenuItem_Click(object? sender, RoutedEventArgs e)
		=> _navigationService.NavigateTo<PlaylistsViewModel>();

	private void CatalogMenuItem_Click(object? sender, RoutedEventArgs e)
		=> _navigationService.NavigateTo<CatalogViewModel>();

	private void DatabaseMenuItem_Click(object? sender, RoutedEventArgs e)
		=> _navigationService.NavigateTo<DatabaseViewModel>();
}