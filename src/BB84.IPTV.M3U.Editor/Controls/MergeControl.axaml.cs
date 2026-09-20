// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for MergeControl.axaml
/// </summary>
/// <remarks>
/// The stored playlists are loaded whenever the control is shown, so a playlist created or deleted
/// elsewhere is picked up.
/// </remarks>
public partial class MergeControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MergeControl"/> class.
	/// </summary>
	public MergeControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (DataContext is MergeViewModel viewModel)
			_ = viewModel.LoadAsync();
	}
}