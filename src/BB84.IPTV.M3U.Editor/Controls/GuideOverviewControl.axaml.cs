// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for GuideOverviewControl.axaml
/// </summary>
/// <remarks>
/// The providers and the first page of guides are loaded whenever the control is shown, so a
/// catalog imported elsewhere is picked up. The load cannot be awaited here, so it reports a
/// failure itself.
/// </remarks>
public partial class GuideOverviewControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GuideOverviewControl"/> class.
	/// </summary>
	public GuideOverviewControl()
		=> InitializeComponent();

	private GuideOverviewViewModel? ViewModel
		=> DataContext as GuideOverviewViewModel;

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (ViewModel is { } viewModel)
			_ = viewModel.LoadAndReportAsync();
	}

	/// <summary>
	/// Searches on return, so the text box does not need the button.
	/// </summary>
	private void OnSearchTextBoxKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key is not Key.Enter || ViewModel is not { } viewModel)
			return;

		if (viewModel.SearchCommand.CanExecute())
			_ = viewModel.SearchCommand.ExecuteAsync();

		e.Handled = true;
	}

	/// <summary>
	/// Takes the sort of a column into the query, so it covers every guide the search found and not
	/// only the page that is shown.
	/// </summary>
	/// <remarks>
	/// The grid applies the sort after this event and cannot be stopped, so the direction is read
	/// once it is in place. Only the first column counts, a shift click adds more than the query
	/// orders by.
	/// </remarks>
	private void OnGuidesSorting(object? sender, DataGridColumnEventArgs e)
	{
		string? sortMemberPath = e.Column.SortMemberPath;
		Dispatcher.UIThread.Post(() => ApplySort(sortMemberPath));
	}

	private void ApplySort(string? sortMemberPath)
	{
		if (ViewModel is not { } viewModel || !Enum.TryParse(sortMemberPath, out GuideSortColumn column))
			return;

		DataGridSortDescription? description = GuideDataGrid.CollectionView?.SortDescriptions
			.FirstOrDefault(candidate => candidate.HasPropertyPath && candidate.PropertyPath == sortMemberPath);

		bool descending = description?.Direction is ListSortDirection.Descending;

		_ = viewModel.SortAsync(column, descending);
	}
}
