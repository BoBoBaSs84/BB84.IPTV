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
public sealed class CatalogControlTests
{
	[TestMethod]
	public async Task ShouldShowTheCatalogWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			CatalogViewModel viewModel = ViewModelFactory.CreateCatalog();
			CatalogControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				// The control loads the filters and the custom channels when it gets its data context.
				UiTest.Settle();

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(2, viewModel.Countries);
				Assert.HasCount(1, viewModel.CustomChannels);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheFilterComboBoxShouldShowTheNamesOfTheValues()
		=> await UiTest.RunAsync(() =>
		{
			CatalogViewModel viewModel = ViewModelFactory.CreateCatalog();
			viewModel.LoadAsync().GetAwaiter().GetResult();

			CatalogControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				ComboBox countries = control.GetVisualDescendants().OfType<ComboBox>().First();
				countries.SelectedIndex = 0;
				UiTest.Settle();

				Assert.AreEqual("Germany", viewModel.SelectedCountry?.ToString());
				Assert.AreEqual("DE", viewModel.SelectedCountry?.Code);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task ThePagingButtonsShouldFollowTheResult()
		=> await UiTest.RunAsync(() =>
		{
			// Two pages: the mock reports more channels than fit on one page.
			CatalogViewModel viewModel = ViewModelFactory.CreateCatalog(CatalogViewModel.PageSize + 1);
			viewModel.LoadAsync().GetAwaiter().GetResult();

			CatalogControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				Button previous = FindButton(control, "< Previous");
				Button next = FindButton(control, "Next >");

				Assert.IsFalse(previous.IsEffectivelyEnabled);
				Assert.IsFalse(next.IsEffectivelyEnabled);

				viewModel.SearchCommand.ExecuteAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				Assert.IsFalse(previous.IsEffectivelyEnabled);
				Assert.IsTrue(next.IsEffectivelyEnabled);

				viewModel.NextPageCommand.ExecuteAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				Assert.AreEqual(2, viewModel.PageNumber);
				Assert.IsTrue(previous.IsEffectivelyEnabled);
				Assert.IsFalse(next.IsEffectivelyEnabled);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task AddingAChannelShouldNeedAnOpenPlaylist()
		=> await UiTest.RunAsync(() =>
		{
			CatalogViewModel viewModel = ViewModelFactory.CreateCatalog();
			viewModel.LoadAsync().GetAwaiter().GetResult();
			viewModel.SearchCommand.ExecuteAsync().GetAwaiter().GetResult();

			CatalogControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				Button add = FindButton(control, "Add to Playlist");

				Assert.IsFalse(add.IsEffectivelyEnabled);

				viewModel.Editor.LoadAsync(1, "Mine").GetAwaiter().GetResult();
				UiTest.Settle();

				Assert.IsTrue(add.IsEffectivelyEnabled);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheCustomChannelDetailShouldFollowTheSelection()
		=> await UiTest.RunAsync(() =>
		{
			CatalogViewModel viewModel = ViewModelFactory.CreateCatalog();
			viewModel.LoadAsync().GetAwaiter().GetResult();

			CatalogControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				Assert.IsTrue(viewModel.SelectedCustomChannelVisible);

				viewModel.SelectedCustomChannel = null;
				UiTest.Settle();

				Assert.IsFalse(viewModel.SelectedCustomChannelVisible);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	private static Button FindButton(Control control, string content)
		=> control.GetVisualDescendants()
			.OfType<Button>()
			.First(button => Equals(button.Content, content));
}