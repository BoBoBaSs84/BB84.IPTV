// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using Avalonia.Controls;
using Avalonia.Interactivity;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for PlaylistsControl.axaml
/// </summary>
/// <remarks>
/// The list selection is not bound, because opening a playlist can be cancelled by the user
/// (unsaved changes), and the list then has to return to the playlist that is still open.
/// <para>
/// The handler of the view model is only registered while the control is loaded. The view model
/// lives as long as the application, and navigating away drops the control without changing its
/// data context, so a handler that stays registered would keep the control and its list alive for
/// the rest of the session.
/// </para>
/// </remarks>
public partial class PlaylistsControl : UserControl
{
	private PlaylistsViewModel? _viewModel;
	private bool _synchronizing;
	private bool _attached;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistsControl"/> class.
	/// </summary>
	public PlaylistsControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		Detach();

		_viewModel = DataContext as PlaylistsViewModel;

		if (IsLoaded)
			Attach();

		SynchronizeSelection();
	}

	/// <inheritdoc/>
	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		Attach();
		SynchronizeSelection();
	}

	/// <inheritdoc/>
	protected override void OnUnloaded(RoutedEventArgs e)
	{
		Detach();

		base.OnUnloaded(e);
	}

	private void Attach()
	{
		if (_viewModel is null || _attached)
			return;

		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
		_attached = true;
	}

	private void Detach()
	{
		if (_viewModel is null || !_attached)
			return;

		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
		_attached = false;
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
