// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class DatabaseServiceTests
{
	[TestMethod]
	public async Task SynchronizeAsyncShouldSkipAListThatCameBackEmpty()
	{
		CatalogSyncResponse response = await _sut
			.SynchronizeAsync(TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(response);
		Assert.HasCount(8, response.Kinds);
		Assert.AreEqual(8, response.SkippedCount);
		Assert.IsFalse(response.IsSuccess);
		Assert.IsFalse(response.HasChanges);
		Assert.IsTrue(response.Kinds.All(kind => kind.Outcome is CatalogSyncOutcome.Skipped));

		// An empty list is a failed response as well, so the user is told and nothing is deleted.
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<WarningOccuredEvent>()), Times.Exactly(8));
	}

	[TestMethod]
	public async Task SynchronizeAsyncShouldReportItsProgressOncePerList()
	{
		_ = await _sut.SynchronizeAsync(TestContext.CancellationToken).ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<CatalogSyncProgressEvent>()), Times.Exactly(8));

		// The status bar of the main window follows the run, like it follows the logo cache.
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ProgressChangedEvent>()), Times.Exactly(8));
		_eventServiceMock.Verify(x => x.Publish(It.Is<ProgressChangedEvent>(e => e.Value == 100)), Times.Once);
	}

	[TestMethod]
	public async Task SynchronizeAsyncShouldStopWhenItIsCancelled()
	{
		_webServiceMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync([new CategoryResponse("news", "News", "What happens.")]);

		using CancellationTokenSource tokenSource = new();
		await tokenSource.CancelAsync().ConfigureAwait(false);

		_ = await Assert.ThrowsAsync<OperationCanceledException>(
			() => _sut.SynchronizeAsync(tokenSource.Token)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetCatalogStatusAsyncShouldAnswerForEveryList()
	{
		IReadOnlyList<CatalogStatusResponse> statuses = await _sut
			.GetCatalogStatusAsync(TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(Enum.GetValues<CatalogKind>().Length, statuses);
		Assert.IsTrue(statuses.All(status => !status.IsImported));
	}

	public TestContext TestContext { get; set; } = default!;
}
