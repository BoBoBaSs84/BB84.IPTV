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
/// Interaction logic for GuideControl.axaml
/// </summary>
/// <remarks>
/// The stored playlists are loaded whenever the control is shown, so a playlist created or deleted
/// elsewhere is picked up. The load cannot be awaited here, so it reports a failure itself.
/// </remarks>
public partial class GuideControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GuideControl"/> class.
	/// </summary>
	public GuideControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (DataContext is GuideViewModel viewModel)
			_ = viewModel.LoadAndReportAsync();
	}

	/// <summary>
	/// Takes the guide a row of the site browser holds into the selected mapping, so a double click
	/// does what the apply button does.
	/// </summary>
	private void OnSiteChannelDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (DataContext is GuideViewModel viewModel && viewModel.ApplySiteChannelCommand.CanExecute())
			viewModel.ApplySiteChannelCommand.Execute();
	}
}
