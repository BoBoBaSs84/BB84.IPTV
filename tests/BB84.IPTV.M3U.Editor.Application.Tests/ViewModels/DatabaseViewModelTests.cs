// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Windows.Input;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class DatabaseViewModelTests : IDisposable
{
	private readonly Mock<IDatabaseService> _databaseServiceMock = new();
	private readonly Mock<ILogoService> _logoServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly DatabaseViewModel _sut;

	public DatabaseViewModelTests()
	{
		_logoServiceMock.Setup(x => x.GetStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new LogoCacheStatusResponse { TotalCount = 10, CachedCount = 4, CachedBytes = 2048 });

		_databaseServiceMock.Setup(x => x.GetCatalogStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		_sut = new DatabaseViewModel(_eventServiceMock.Object, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());
	}

	/// <summary>
	/// The view model stops a running logo download when it is disposed.
	/// </summary>
	public void Dispose()
		=> _sut.Dispose();

	[TestMethod]
	public async Task CreatingTheDatabaseShouldEnableTheCheck()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		int checkChanged = 0;
		_sut.CheckDatabaseCommand.CanExecuteChanged += (s, e) => checkChanged++;

		Assert.IsFalse(_sut.CheckDatabaseCommand.CanExecute());

		// The catalog is updated, not created, so the update does not wait for a creation.
		Assert.IsTrue(_sut.SynchronizeDatabaseCommand.CanExecute());

		await _sut.CreateDatabaseCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsTrue(_sut.DatabaseCreated);
		Assert.IsTrue(_sut.CheckDatabaseCommand.CanExecute());
		Assert.IsGreaterThan(0, checkChanged);
	}

	[TestMethod]
	public async Task TheUpdateShouldReportWhatItChangedWithALocalizedMessage()
	{
		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CatalogSyncResponse
			{
				Kinds =
				[
					new CatalogKindResponse { Kind = CatalogKind.Category, Outcome = CatalogSyncOutcome.Synchronized, Added = 3 },
					new CatalogKindResponse { Kind = CatalogKind.Channel, Outcome = CatalogSyncOutcome.Synchronized, Updated = 4, Removed = 1 }
				]
			});

		await _sut.SynchronizeDatabaseCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsTrue(_sut.DatabaseSynchronized);
		Assert.AreEqual(Resources.DatabaseSyncSucceeded.FormatMessage(3, 4, 1), _sut.SyncStatusMessage);

		// The update stays available, a catalog is brought up to date more than once.
		Assert.IsTrue(_sut.SynchronizeDatabaseCommand.CanExecute());
	}

	[TestMethod]
	public async Task TheUpdateShouldReportThatNothingChanged()
	{
		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CatalogSyncResponse
			{
				Kinds = [new CatalogKindResponse { Kind = CatalogKind.Category, Outcome = CatalogSyncOutcome.Synchronized, Unchanged = 12 }]
			});

		await _sut.SynchronizeDatabaseCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseSyncUnchanged, _sut.SyncStatusMessage);
	}

	[TestMethod]
	public async Task TheUpdateShouldReportTheListsItLeftAlone()
	{
		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CatalogSyncResponse
			{
				Kinds =
				[
					new CatalogKindResponse { Kind = CatalogKind.Category, Outcome = CatalogSyncOutcome.Synchronized, Added = 2 },
					new CatalogKindResponse { Kind = CatalogKind.Guide, Outcome = CatalogSyncOutcome.Skipped }
				]
			});

		await _sut.SynchronizeDatabaseCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseSyncSkipped.FormatMessage(1), _sut.SyncStatusMessage);
	}

	[TestMethod]
	public async Task TheCatalogStatusShouldHoldWhatIsKnownPerList()
	{
		_databaseServiceMock.Setup(x => x.GetCatalogStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(
			[
				new CatalogStatusResponse
				{
					Kind = CatalogKind.Category,
					FirstImported = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
					LastChecked = new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc),
					LastChanged = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
					Added = 1
				},
				new CatalogStatusResponse { Kind = CatalogKind.Guide }
			]);

		await _sut.LoadCatalogStatusAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.HasCount(2, _sut.CatalogStatuses);
		Assert.AreEqual(Resources.CatalogKindCategory, _sut.CatalogStatuses[0].Kind);
		Assert.AreEqual("1", _sut.CatalogStatuses[0].Added);

		// A list that was never read shows no date and no counter.
		Assert.AreEqual(Resources.CatalogStatusNever, _sut.CatalogStatuses[1].FirstImported);
		Assert.AreEqual(string.Empty, _sut.CatalogStatuses[1].Added);
	}

	[TestMethod]
	public async Task AFailedUpdateShouldPublishALocalizedError()
	{
		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no connection"));

		ErrorOccuredEvent reported = await ExecuteAndReadErrorAsync(_sut.SynchronizeDatabaseCommand).ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseSyncFailed, reported.Message);
		Assert.IsNotNull(reported.Exception);
		Assert.AreEqual(Resources.DatabaseSyncFailed, _sut.SyncStatusMessage);
		Assert.AreEqual(0, _sut.SyncProgress);
	}

	[TestMethod]
	public async Task AFailedCheckShouldPublishALocalizedError()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_databaseServiceMock.Setup(x => x.CheckDatabaseAvailabilityAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no connection"));

		await _sut.CreateDatabaseCommand.ExecuteAsync(TestContext.CancellationToken).ConfigureAwait(false);
		ErrorOccuredEvent reported = await ExecuteAndReadErrorAsync(_sut.CheckDatabaseCommand).ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseCheckFailed, reported.Message);
		Assert.IsNotNull(reported.Exception);
	}

	/// <summary>
	/// Runs the command the way the UI does, which reports a failure to the error handler of the
	/// command instead of throwing, and reads the published error.
	/// </summary>
	/// <param name="command">The command to run.</param>
	/// <returns>The error the view model published.</returns>
	private async Task<ErrorOccuredEvent> ExecuteAndReadErrorAsync(ICommand command)
	{
		TaskCompletionSource<ErrorOccuredEvent> published = new();
		_eventServiceMock.Setup(x => x.Publish(It.IsAny<ErrorOccuredEvent>()))
			.Callback((ErrorOccuredEvent @event) => published.TrySetResult(@event));

		command.Execute(null);

		return await published.Task
			.WaitAsync(TimeSpan.FromSeconds(5))
			.ConfigureAwait(false);
	}

	[TestMethod]
	public async Task AFailedLogoCacheShouldReachTheUserInterfaceThread()
	{
		_logoServiceMock.Setup(x => x.GetStatusAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no database"));

		RecordingContext context = new();
		SynchronizationContext? previous = SynchronizationContext.Current;
		DatabaseViewModel viewModel;

		try
		{
			SynchronizationContext.SetSynchronizationContext(context);
			viewModel = new(_eventServiceMock.Object, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(previous);
		}

		using (viewModel)
		{
			// The error callback of a command runs on a thread pool thread; a bound property set
			// there ends the application, so it has to go back to the thread of the interface.
			await Task.Run(viewModel.LoadLogoCacheStatusAndReportAsync, TestContext.CancellationToken).ConfigureAwait(false);

			Assert.AreEqual(1, context.Posts);
			Assert.AreEqual(Resources.LogoCacheFailed, viewModel.LogoStatusMessage);
			Assert.AreEqual(0, viewModel.LogoProgress);
		}
	}

	/// <summary>
	/// Counts what was posted to it and runs it straight away, so a test does not have to wait.
	/// </summary>
	private sealed class RecordingContext : SynchronizationContext
	{
		private int _posts;

		public int Posts
			=> _posts;

		public override void Post(SendOrPostCallback d, object? state)
		{
			_ = Interlocked.Increment(ref _posts);
			d(state);
		}
	}

	[TestMethod]
	public void TheUpdateProgressShouldBeReportedWithALocalizedMessage()
	{
		EventService eventService = new(NullLogger<EventService>.Instance);
		using DatabaseViewModel viewModel = new(eventService, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());

		eventService.Publish(new CatalogSyncProgressEvent(CatalogKind.Channel, 12, 3, 1, 4, 8));

		Assert.AreEqual(50, viewModel.SyncProgress);
		Assert.AreEqual(
			Resources.DatabaseSyncProgressStatus.FormatMessage(Resources.CatalogKindChannel, 12, 3, 1, 4, 8),
			viewModel.SyncStatusMessage);
	}

	public TestContext TestContext { get; set; }
}