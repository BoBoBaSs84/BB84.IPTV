// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.Interactivity;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Represents the playlist control in the IPTV M3U Editor application.
/// </summary>
public partial class PlaylistControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistControl"/> class.
	/// </summary>
	public PlaylistControl()
		=> InitializeComponent();

	private PlaylistViewModel? ViewModel
		=> DataContext as PlaylistViewModel;

	private void AddButton_Click(object? sender, RoutedEventArgs e)
		=> ShowSelectedEntry(vm => vm.AddEntry());

	private void DuplicateButton_Click(object? sender, RoutedEventArgs e)
		=> ShowSelectedEntry(vm => vm.DuplicateSelectedEntry());

	private void RemoveButton_Click(object? sender, RoutedEventArgs e)
		=> ShowSelectedEntry(vm => vm.RemoveSelectedEntry());

	private void MoveUpButton_Click(object? sender, RoutedEventArgs e)
		=> ShowSelectedEntry(vm => vm.MoveSelectedEntry(-1));

	private void MoveDownButton_Click(object? sender, RoutedEventArgs e)
		=> ShowSelectedEntry(vm => vm.MoveSelectedEntry(1));

	private void ShowSelectedEntry(Action<PlaylistViewModel> action)
	{
		if (ViewModel is not { } viewModel)
			return;

		action(viewModel);

		if (viewModel.SelectedEntry is not null)
			EntriesDataGrid.ScrollIntoView(viewModel.SelectedEntry, null);
	}
}