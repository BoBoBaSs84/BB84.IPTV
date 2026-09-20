// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Controls;
using Avalonia.VisualTree;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Controls;
using BB84.IPTV.M3U.Editor.Tests.Infrastructure;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Controls;

[TestClass]
public sealed class MergeControlTests
{
	[TestMethod]
	public async Task ShouldShowTheSourcesWithoutBindingErrors()
		=> await UiTest.RunAsync(() =>
		{
			MergeViewModel viewModel = ViewModelFactory.CreateMerge();
			MergeControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				// The control loads the stored playlists when it gets its data context.
				UiTest.Settle();

				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
				Assert.HasCount(3, viewModel.Sources);
				Assert.IsFalse(viewModel.HasPreview);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task CheckingTwoSourcesShouldEnableThePreviewAndTheMerge()
		=> await UiTest.RunAsync(() =>
		{
			MergeViewModel viewModel = ViewModelFactory.CreateMerge();
			viewModel.LoadAsync().GetAwaiter().GetResult();

			MergeControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				Button preview = FindButton(control, "Preview");
				Button merge = FindButton(control, "Merge");
				CheckBox[] checkBoxes = [.. control.GetVisualDescendants().OfType<CheckBox>()];

				Assert.IsFalse(preview.IsEffectivelyEnabled);
				Assert.IsFalse(merge.IsEffectivelyEnabled);
				Assert.HasCount(3, checkBoxes);

				checkBoxes[0].IsChecked = true;
				UiTest.Settle();

				Assert.IsFalse(merge.IsEffectivelyEnabled, "One playlist is not a merge.");

				checkBoxes[1].IsChecked = true;
				UiTest.Settle();

				Assert.AreEqual(2, viewModel.SelectedCount);
				Assert.IsTrue(preview.IsEffectivelyEnabled);
				Assert.IsTrue(merge.IsEffectivelyEnabled);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task ThePreviewShouldShowTheEntriesAndTheGroups()
		=> await UiTest.RunAsync(() =>
		{
			MergeViewModel viewModel = ViewModelFactory.CreateMerge();
			viewModel.LoadAsync().GetAwaiter().GetResult();
			viewModel.Sources[0].IsSelected = true;
			viewModel.Sources[1].IsSelected = true;

			MergeControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, sink =>
			{
				viewModel.PreviewAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				DataGrid[] grids = [.. control.GetVisualDescendants().OfType<DataGrid>()];

				Assert.IsTrue(viewModel.HasPreview);
				Assert.HasCount(1, viewModel.PreviewEntries);
				Assert.HasCount(2, viewModel.GroupMappings);

				// The group table is only shown once a preview exists.
				Assert.HasCount(2, grids);
				Assert.IsEmpty(sink.Messages, string.Join(Environment.NewLine, sink.Messages));
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task RenamingAGroupShouldReachTheRequest()
		=> await UiTest.RunAsync(() =>
		{
			Mock<IMergeService> mergeServiceMock = new();
			MergeViewModel viewModel = ViewModelFactory.CreateMerge(mergeServiceMock);
			viewModel.LoadAsync().GetAwaiter().GetResult();
			viewModel.Sources[0].IsSelected = true;
			viewModel.Sources[1].IsSelected = true;

			MergeControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				viewModel.PreviewAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				viewModel.GroupMappings[0].TargetGroup = "Öffentlich";
				viewModel.MergeCommand.ExecuteAsync().GetAwaiter().GetResult();
				UiTest.Settle();

				mergeServiceMock.Verify(x => x.MergeAsync(
					It.Is<MergeRequest>(request => request.GroupMappings!["Public"] == "Öffentlich"),
					It.IsAny<CancellationToken>()), Times.Once);
			});
		}).ConfigureAwait(false);

	[TestMethod]
	public async Task TheOrderButtonsShouldMoveTheSelectedSource()
		=> await UiTest.RunAsync(() =>
		{
			MergeViewModel viewModel = ViewModelFactory.CreateMerge();
			viewModel.LoadAsync().GetAwaiter().GetResult();

			MergeControl control = new() { DataContext = viewModel };

			UiTest.InWindow(control, _ =>
			{
				Button up = FindButton(control, "Move Up");
				Button down = FindButton(control, "Move Down");

				Assert.IsFalse(up.IsEffectivelyEnabled, "The first source cannot move up.");
				Assert.IsTrue(down.IsEffectivelyEnabled);

				down.Command!.Execute(null);
				UiTest.Settle();

				Assert.AreEqual("First", viewModel.Sources[1].Name);
				Assert.IsTrue(up.IsEffectivelyEnabled);
			});
		}).ConfigureAwait(false);

	private static Button FindButton(Control control, string content)
		=> control.GetVisualDescendants()
			.OfType<Button>()
			.First(button => Equals(button.Content, content));
}