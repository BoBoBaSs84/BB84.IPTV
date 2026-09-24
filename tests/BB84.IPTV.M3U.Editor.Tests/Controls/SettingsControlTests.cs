// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class SettingsControlTests
{
	[TestMethod]
	public async Task ShouldShowTheSettingsWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			SettingsViewModel viewModel = ViewModelFactory.CreateSettings();
			SettingsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink
				=> Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages)));
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheDataPathBoxesShouldShowTheDirectoriesInUseAsPlaceholder()
		=> await UiTest.RunAsync(() =>
		{
			SettingsViewModel viewModel = ViewModelFactory.CreateSettings();
			SettingsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				string[] placeholders = [.. control.GetVisualDescendants()
					.OfType<TextBox>()
					.Select(box => box.PlaceholderText ?? string.Empty)];

				Assert.Contains(viewModel.DataDirectoryInUse, placeholders);
				Assert.Contains(viewModel.LogoDirectoryInUse, placeholders);
				Assert.Contains(viewModel.LogDirectoryInUse, placeholders);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheLogoPolicyShouldBeBoundToTheLogoSettings()
		=> await UiTest.RunAsync(() =>
		{
			SettingsViewModel viewModel = ViewModelFactory.CreateSettings();
			SettingsControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				CheckBox useLocalPaths = control.GetVisualDescendants()
					.OfType<CheckBox>()
					.First(box => Equals(box.Content, Properties.Resources.SettingsControl_UseLocalPathsOnExportCheckBox_Content));
				NumericUpDown maxParallelDownloads = control.GetVisualDescendants()
					.OfType<NumericUpDown>()
					.First(input => input.Maximum == SettingsViewModel.MaxParallelDownloadsLimit);

				Assert.AreEqual(viewModel.Logo.UseLocalPathsOnExport, useLocalPaths.IsChecked);
				Assert.AreEqual(viewModel.Logo.MaxParallelDownloads, (int)maxParallelDownloads.Value!);

				useLocalPaths.IsChecked = false;
				maxParallelDownloads.Value = 8;
				UiTest.Settle();

				Assert.IsFalse(viewModel.Logo.UseLocalPathsOnExport);
				Assert.AreEqual(8, viewModel.Logo.MaxParallelDownloads);
			});
		}).ConfigureAwait(false);
}