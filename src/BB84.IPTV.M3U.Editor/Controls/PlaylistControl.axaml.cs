// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

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

	// The grid applies the sort after the event and drops the selection, so both are handled afterwards.
	private void OnEntriesSorting(object? sender, DataGridColumnEventArgs e)
	{
		IEntry? selected = ViewModel?.SelectedEntry;
		Dispatcher.UIThread.Post(() => UpdateSortState(selected));
	}

	// A sorted view no longer shows the playlist order, so reordering is off while it is sorted.
	private void UpdateSortState(IEntry? selected)
	{
		if (ViewModel is not { } viewModel)
			return;

		viewModel.IsSorted = EntriesDataGrid.CollectionView?.SortDescriptions.Count > 0;
		viewModel.SelectedEntry ??= selected;
	}

	private void ShowSelectedEntry(Action<PlaylistViewModel> action)
	{
		if (ViewModel is not { } viewModel)
			return;

		action(viewModel);

		if (viewModel.SelectedEntry is not null)
			EntriesDataGrid.ScrollIntoView(viewModel.SelectedEntry, null);
	}
}