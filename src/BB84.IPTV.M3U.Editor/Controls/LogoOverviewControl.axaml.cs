// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.Input;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for LogoOverviewControl.axaml
/// </summary>
/// <remarks>
/// The playlists and the first page of logos are loaded whenever the control is shown, so a catalog
/// imported elsewhere is picked up. The load cannot be awaited here, so it reports a failure itself.
/// </remarks>
public partial class LogoOverviewControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LogoOverviewControl"/> class.
	/// </summary>
	public LogoOverviewControl()
		=> InitializeComponent();

	private LogoOverviewViewModel? ViewModel
		=> DataContext as LogoOverviewViewModel;

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
}
