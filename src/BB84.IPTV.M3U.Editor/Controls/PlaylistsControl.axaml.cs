// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using Avalonia.Controls;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for PlaylistsControl.axaml
/// </summary>
/// <remarks>
/// The list selection is not bound, because opening a playlist can be cancelled by the user
/// (unsaved changes), and the list then has to return to the playlist that is still open.
/// </remarks>
public partial class PlaylistsControl : UserControl
{
	private PlaylistsViewModel? _viewModel;
	private bool _synchronizing;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistsControl"/> class.
	/// </summary>
	public PlaylistsControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (_viewModel is not null)
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = DataContext as PlaylistsViewModel;

		if (_viewModel is not null)
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;

		SynchronizeSelection();
	}

	private async void PlaylistList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (_synchronizing || _viewModel is null || PlaylistList.SelectedItem is not PlaylistItemViewModel item)
			return;

		await _viewModel.OpenAsync(item).ConfigureAwait(true);

		// Opened, cancelled or failed: the list shows the playlist that is open now.
		SynchronizeSelection();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(PlaylistsViewModel.CurrentPlaylist))
			SynchronizeSelection();
	}

	private void SynchronizeSelection()
	{
		_synchronizing = true;
		try
		{
			PlaylistList.SelectedItem = _viewModel?.CurrentPlaylist;
		}
		finally
		{
			_synchronizing = false;
		}
	}
}