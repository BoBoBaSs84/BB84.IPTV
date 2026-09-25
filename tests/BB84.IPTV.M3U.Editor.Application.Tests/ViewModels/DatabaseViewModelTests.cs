using System.Windows.Input;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

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

		_sut = new DatabaseViewModel(_eventServiceMock.Object, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());
	}

	/// <summary>
	/// The view model stops a running logo download when it is disposed.
	/// </summary>
	public void Dispose()
		=> _sut.Dispose();

	[TestMethod]
	public async Task CreatingTheDatabaseShouldEnableCheckAndImport()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		int checkChanged = 0;
		int importChanged = 0;
		_sut.CheckDatabaseCommand.CanExecuteChanged += (s, e) => checkChanged++;
		_sut.ImportDatabaseCommand.CanExecuteChanged += (s, e) => importChanged++;

		Assert.IsFalse(_sut.CheckDatabaseCommand.CanExecute());
		Assert.IsFalse(_sut.ImportDatabaseCommand.CanExecute());

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.IsTrue(_sut.DatabaseCreated);
		Assert.IsTrue(_sut.CheckDatabaseCommand.CanExecute());
		Assert.IsTrue(_sut.ImportDatabaseCommand.CanExecute());
		Assert.IsGreaterThan(0, checkChanged);
		Assert.IsGreaterThan(0, importChanged);
	}

	[TestMethod]
	public async Task TheImportShouldReportItsResultWithALocalizedMessage()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_databaseServiceMock.Setup(x => x.ImportDatabaseAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new DatabaseImportResponse { CategoriesImported = 3, CountriesImported = 4 });

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.ImportDatabaseCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseImportSucceeded.FormatMessage(7), _sut.ImportStatusMessage);
	}

	[TestMethod]
	public async Task TheImportShouldReportAnEmptyResultWithALocalizedMessage()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_databaseServiceMock.Setup(x => x.ImportDatabaseAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new DatabaseImportResponse());

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.ImportDatabaseCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseImportWithoutRecords, _sut.ImportStatusMessage);
	}

	[TestMethod]
	public async Task AFailedImportShouldPublishALocalizedError()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_databaseServiceMock.Setup(x => x.ImportDatabaseAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no connection"));

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);
		ErrorOccuredEvent reported = await ExecuteAndReadErrorAsync(_sut.ImportDatabaseCommand).ConfigureAwait(false);

		Assert.AreEqual(Resources.DatabaseImportFailed, reported.Message);
		Assert.IsNotNull(reported.Exception);
		Assert.AreEqual(Resources.DatabaseImportFailed, _sut.ImportStatusMessage);
		Assert.AreEqual(0, _sut.ImportProgress);
	}

	[TestMethod]
	public async Task AFailedCheckShouldPublishALocalizedError()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		_databaseServiceMock.Setup(x => x.CheckDatabaseAvailabilityAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no connection"));

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);
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
			await Task.Run(viewModel.LoadLogoCacheStatusAndReportAsync).ConfigureAwait(false);

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
	public void TheImportProgressShouldBeReportedWithALocalizedMessage()
	{
		EventService eventService = new(NullLogger<EventService>.Instance);
		using DatabaseViewModel viewModel = new(eventService, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());

		eventService.Publish(new DatabaseImportProgressEvent("Channels", 12, 4, 8));

		Assert.AreEqual(50, viewModel.ImportProgress);
		Assert.AreEqual(Resources.DatabaseImportProgressStatus.FormatMessage("Channels", 12, 4, 8), viewModel.ImportStatusMessage);
	}
}