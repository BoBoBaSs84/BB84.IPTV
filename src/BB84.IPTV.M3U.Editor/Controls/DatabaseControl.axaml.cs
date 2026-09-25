// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for DatabaseControl.axaml
/// </summary>
public partial class DatabaseControl : UserControl
{
	/// <summary>
	/// Initializes an instance of the <see cref="DatabaseControl"/> class.
	/// </summary>
	public DatabaseControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	/// <remarks>
	/// The state of the logo cache is read whenever the screen is shown, so an import or a run
	/// elsewhere is picked up. The load cannot be awaited here, so it reports a failure itself.
	/// </remarks>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (DataContext is DatabaseViewModel viewModel)
			_ = viewModel.LoadLogoCacheStatusAndReportAsync();
	}
}