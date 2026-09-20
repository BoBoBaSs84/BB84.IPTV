// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;

using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for CatalogControl.axaml
/// </summary>
/// <remarks>
/// The filter values and the custom channels are loaded whenever the control is shown, so an
/// imported catalog is picked up without a restart. The load cannot be awaited here, so it reports
/// a failure itself.
/// </remarks>
public partial class CatalogControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CatalogControl"/> class.
	/// </summary>
	public CatalogControl()
		=> InitializeComponent();

	/// <inheritdoc/>
	protected override void OnDataContextChanged(EventArgs e)
	{
		base.OnDataContextChanged(e);

		if (DataContext is CatalogViewModel viewModel)
			_ = viewModel.LoadAndReportAsync();
	}
}
