// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the view model for managing database operations such as checking, creating, and importing the database.
/// </summary>
public sealed class DatabaseViewModel : ViewModelBase, INavigateable, IDisposable
{
	private readonly IEventService _eventService;
	private readonly IDatabaseService _databaseService;
	private readonly ILogoService _logoService;
	private readonly ApplicationSettings _settings;
	private CancellationTokenSource? _logoCacheCancellation;
	private LogoCacheStatusResponse _logoCacheStatus = new();
	private bool _logosCaching;
	private int _logoProgress;
	private string _logoStatusMessage = string.Empty;
	private bool _databaseChecked;
	private bool _databaseChecking;
	private bool _databaseCreated;
	private bool _databaseCreating;
	private bool _databaseSynchronized;
	private bool _databaseSynchronizing;
	private int _syncProgress;
	private int _syncProgressMaximum = 100;
	private string _syncStatusMessage = string.Empty;
	private AsyncActionCommand? _checkDatabaseCommand;
	private AsyncActionCommand? _createDatabaseCommand;
	private AsyncActionCommand? _synchronizeDatabaseCommand;
	private AsyncActionCommand? _cacheLogosCommand;
	private ActionCommand? _cancelLogoCacheCommand;
	private AsyncActionCommand? _clearLogoCacheCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="DatabaseViewModel"/> class.
	/// </summary>
	/// <param name="eventService">The event service for subscribing and publishing events.</param>
	/// <param name="databaseService">The database service for managing database operations.</param>
	/// <param name="logoService">The service that keeps the channel logos on disk.</param>
	/// <param name="settings">The application settings, which say how many logos are downloaded at once.</param>
	public DatabaseViewModel(IEventService eventService, IDatabaseService databaseService, ILogoService logoService, ApplicationSettings settings)
	{
		_eventService = eventService;
		_databaseService = databaseService;
		_logoService = logoService;
		_settings = settings;

		_eventService.Subscribe<CatalogSyncProgressEvent>(OnCatalogSyncProgress);
		_eventService.Subscribe<LogoCacheProgressEvent>(OnLogoCacheProgress);

		// The commands depend on the database state, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets what is known about the synchronization of each list of the catalog.
	/// </summary>
	public ObservableCollection<CatalogStatusItemViewModel> CatalogStatuses { get; } = [];

	/// <summary>
	/// Indicates whether the database has been checked or not.
	/// </summary>
	public bool DatabaseChecked
	{
		get => _databaseChecked;
		set => SetProperty(ref _databaseChecked, value);
	}

	/// <summary>
	/// Indicates whether the database is being checked or not.
	/// </summary>
	public bool DatabaseChecking
	{
		get => _databaseChecking;
		set => SetProperty(ref _databaseChecking, value);
	}

	/// <summary>
	/// Indicates whether the database has been created or not.
	/// </summary>
	public bool DatabaseCreated
	{
		get => _databaseCreated;
		private set => SetProperty(ref _databaseCreated, value);
	}

	/// <summary>
	/// Indicates whether the database is being created or not.
	/// </summary>
	public bool DatabaseCreating
	{
		get => _databaseCreating;
		private set => SetProperty(ref _databaseCreating, value);
	}

	/// <summary>
	/// Indicates whether the catalog was synchronized in this session.
	/// </summary>
	public bool DatabaseSynchronized
	{
		get => _databaseSynchronized;
		private set => SetProperty(ref _databaseSynchronized, value);
	}

	/// <summary>
	/// Indicates whether the catalog is being synchronized.
	/// </summary>
	public bool DatabaseSynchronizing
	{
		get => _databaseSynchronizing;
		private set => SetProperty(ref _databaseSynchronizing, value);
	}

	/// <summary>
	/// Gets the current synchronization progress value (0-100).
	/// </summary>
	public int SyncProgress
	{
		get => _syncProgress;
		private set => SetProperty(ref _syncProgress, value);
	}

	/// <summary>
	/// Gets the maximum value for the synchronization progress (typically 100).
	/// </summary>
	public int SyncProgressMaximum
	{
		get => _syncProgressMaximum;
		private set => SetProperty(ref _syncProgressMaximum, value);
	}

	/// <summary>
	/// Gets what the synchronization is doing, e.g. which list is read.
	/// </summary>
	public string SyncStatusMessage
	{
		get => _syncStatusMessage;
		private set => SetProperty(ref _syncStatusMessage, value);
	}

	/// <summary>
	/// The command to check the database.
	/// </summary>
	public IAsyncActionCommand CheckDatabaseCommand
		=> _checkDatabaseCommand ??= new AsyncActionCommand(CheckDatabaseAsync, CanCheckDatabase, CheckDataBaseFailed);

	/// <summary>
	/// The command to create the database.
	/// </summary>
	public IAsyncActionCommand CreateDatabaseCommand
		=> _createDatabaseCommand ??= new AsyncActionCommand(CreateDatabaseAsync, CanCreateDatabase, CreateDataBaseFailed);

	/// <summary>
	/// The command that brings the catalog in line with iptv-org.
	/// </summary>
	public IAsyncActionCommand SynchronizeDatabaseCommand
		=> _synchronizeDatabaseCommand ??= new AsyncActionCommand(SynchronizeDatabaseAsync, CanSynchronizeDatabase, SynchronizeDatabaseFailed);

	private async Task CheckDatabaseAsync()
	{
		try
		{
			DatabaseChecking = true;

			DatabaseChecked = await _databaseService
				.CheckDatabaseAvailabilityAsync();
		}
		finally
		{
			DatabaseChecking = false;
		}
	}

	private bool CanCheckDatabase()
		=> _databaseChecked.IsFalse() && _databaseChecking.IsFalse() && _databaseCreated.IsTrue();

	private void CheckDataBaseFailed(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.DatabaseCheckFailed, exception));

	private async Task CreateDatabaseAsync()
	{
		try
		{
			DatabaseCreating = true;

			DatabaseCreated = await _databaseService
				.CreateDatabaseAsync();
		}
		finally
		{
			DatabaseCreating = false;
		}
	}

	private bool CanCreateDatabase()
		=> _databaseCreated.IsFalse() && _databaseCreating.IsFalse();

	private void CreateDataBaseFailed(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.DatabaseCreateFailed, exception));

	private async Task SynchronizeDatabaseAsync()
	{
		try
		{
			DatabaseSynchronizing = true;
			SyncProgress = 0;
			SyncStatusMessage = Resources.DatabaseSyncStarted;

			CatalogSyncResponse response = await _databaseService
				.SynchronizeAsync()
				.ConfigureAwait(true);

			DatabaseSynchronized = true;
			SyncStatusMessage = GetSyncStatusMessage(response);
		}
		finally
		{
			DatabaseSynchronizing = false;
			await LoadCatalogStatusAsync().ConfigureAwait(true);
			await LoadLogoCacheStatusAsync().ConfigureAwait(true);
		}
	}

	/// <summary>
	/// Tells what the run did: what changed, what was left alone, or that nothing moved.
	/// </summary>
	private static string GetSyncStatusMessage(CatalogSyncResponse response)
	{
		if (response.SkippedCount > 0)
			return Resources.DatabaseSyncSkipped.FormatMessage(response.SkippedCount);

		return response.HasChanges
			? Resources.DatabaseSyncSucceeded.FormatMessage(response.TotalAdded, response.TotalUpdated, response.TotalRemoved)
			: Resources.DatabaseSyncUnchanged;
	}

	// The catalog is updated, not created, so a finished run does not disable the command.
	private bool CanSynchronizeDatabase()
		=> _databaseSynchronizing.IsFalse();

	private void SynchronizeDatabaseFailed(Exception exception)
	{
		_eventService.Publish(new ErrorOccuredEvent(Resources.DatabaseSyncFailed, exception));

		Invoke(() =>
		{
			SyncStatusMessage = Resources.DatabaseSyncFailed;
			SyncProgress = 0;
		});
	}

	private void RaiseCommandStatesChanged()
	{
		_checkDatabaseCommand?.RaiseCanExecuteChanged();
		_createDatabaseCommand?.RaiseCanExecuteChanged();
		_synchronizeDatabaseCommand?.RaiseCanExecuteChanged();
		_cacheLogosCommand?.RaiseCanExecuteChanged();
		_cancelLogoCacheCommand?.RaiseCanExecuteChanged();
		_clearLogoCacheCommand?.RaiseCanExecuteChanged();
	}

	/// <summary>
	/// Gets what the logo cache holds.
	/// </summary>
	[NotifyChanged(nameof(LogoCacheMissingMessage))]
	public LogoCacheStatusResponse LogoCacheStatus
	{
		get => _logoCacheStatus;
		private set => SetProperty(ref _logoCacheStatus, value);
	}

	/// <summary>
	/// Gets how many logos are not cached yet, e.g. "42 missing".
	/// </summary>
	public string LogoCacheMissingMessage
		=> Resources.LogoCacheMissing.FormatMessage(LogoCacheStatus.MissingCount);

	/// <summary>
	/// Indicates whether logos are being downloaded.
	/// </summary>
	public bool LogosCaching
	{
		get => _logosCaching;
		private set => SetProperty(ref _logosCaching, value);
	}

	/// <summary>
	/// Gets the progress of the running download in percent.
	/// </summary>
	public int LogoProgress
	{
		get => _logoProgress;
		private set => SetProperty(ref _logoProgress, value);
	}

	/// <summary>
	/// Gets what the logo cache is doing, e.g. how many logos are cached.
	/// </summary>
	public string LogoStatusMessage
	{
		get => _logoStatusMessage;
		private set => SetProperty(ref _logoStatusMessage, value);
	}

	/// <summary>
	/// The command to download the logos that are not cached yet.
	/// </summary>
	public IAsyncActionCommand CacheLogosCommand
		=> _cacheLogosCommand ??= new AsyncActionCommand(CacheLogosAsync, () => !LogosCaching, LogoOperationFailed);

	/// <summary>
	/// The command to stop a running download, the logos downloaded so far are kept.
	/// </summary>
	public IActionCommand CancelLogoCacheCommand
		=> _cancelLogoCacheCommand ??= new ActionCommand(() => _logoCacheCancellation?.Cancel(), () => LogosCaching);

	/// <summary>
	/// The command to delete every cached logo.
	/// </summary>
	public IAsyncActionCommand ClearLogoCacheCommand
		=> _clearLogoCacheCommand ??= new AsyncActionCommand(ClearLogoCacheAsync, () => !LogosCaching, LogoOperationFailed);

	/// <summary>
	/// Reads what the logo cache holds, e.g. when the screen is shown.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadLogoCacheStatusAsync(CancellationToken cancellationToken = default)
	{
		LogoCacheStatus = await _logoService
			.GetStatusAsync(cancellationToken)
			.ConfigureAwait(true);

		LogoStatusMessage = Resources.LogoCacheStatus.FormatMessage(LogoCacheStatus.CachedCount, LogoCacheStatus.TotalCount);
	}

	/// <summary>
	/// Reads what is known about the synchronization of the catalog, one row per list.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadCatalogStatusAsync(CancellationToken cancellationToken = default)
	{
		IReadOnlyList<CatalogStatusResponse> statuses = await _databaseService
			.GetCatalogStatusAsync(cancellationToken)
			.ConfigureAwait(true);

		CatalogStatuses.Clear();
		foreach (CatalogStatusResponse status in statuses)
			CatalogStatuses.Add(new CatalogStatusItemViewModel(status));
	}

	/// <summary>
	/// Reads the catalog status and the logo cache status, and reports a failure instead of
	/// throwing, for callers that cannot await.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadStatusAndReportAsync()
	{
		try
		{
			await LoadCatalogStatusAsync().ConfigureAwait(true);
			await LoadLogoCacheStatusAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			LogoOperationFailed(exception);
		}
	}

	/// <summary>
	/// Reads the status and reports a failure instead of throwing, for callers that cannot await.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadLogoCacheStatusAndReportAsync()
	{
		try
		{
			await LoadLogoCacheStatusAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			LogoOperationFailed(exception);
		}
	}

	/// <summary>
	/// Stops a running download and releases what it needs.
	/// </summary>
	public void Dispose()
	{
		_logoCacheCancellation?.Dispose();
		_logoCacheCancellation = null;
	}

	private async Task CacheLogosAsync()
	{
		CancellationTokenSource tokenSource = new();

		_logoCacheCancellation?.Dispose();
		_logoCacheCancellation = tokenSource;

		try
		{
			LogosCaching = true;
			LogoProgress = 0;

			int downloaded = await _logoService
				.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = _settings.Logo.MaxParallelDownloads }, tokenSource.Token)
				.ConfigureAwait(true);

			LogoStatusMessage = Resources.LogoCacheDownloaded.FormatMessage(downloaded);
		}
		finally
		{
			LogosCaching = false;

			// The run is through, so nothing has to be cancelled any more. A run that was started
			// again in the meantime owns the field and keeps its own source.
			if (ReferenceEquals(_logoCacheCancellation, tokenSource))
			{
				_logoCacheCancellation = null;
				tokenSource.Dispose();
			}

			await LoadLogoCacheStatusAsync().ConfigureAwait(true);
		}
	}

	private async Task ClearLogoCacheAsync()
	{
		int deleted = await _logoService
			.ClearAsync()
			.ConfigureAwait(true);

		await LoadLogoCacheStatusAsync().ConfigureAwait(true);
		LogoStatusMessage = Resources.LogoCacheCleared.FormatMessage(deleted);
	}

	private void LogoOperationFailed(Exception exception)
	{
		_eventService.Publish(new ErrorOccuredEvent(Resources.LogoCacheFailed, exception));

		Invoke(() =>
		{
			LogoStatusMessage = Resources.LogoCacheFailed;
			LogoProgress = 0;
		});
	}

	private void OnLogoCacheProgress(LogoCacheProgressEvent @event)
		=> Invoke(() => UpdateLogoProgress(@event));

	private void UpdateLogoProgress(LogoCacheProgressEvent @event)
	{
		LogoProgress = @event.ProgressPercentage;
		LogoStatusMessage = Resources.LogoCacheProgress.FormatMessage(@event.ProcessedCount, @event.TotalCount);
	}

	private void OnCatalogSyncProgress(CatalogSyncProgressEvent @event)
		=> Invoke(() => UpdateSyncProgress(@event));

	private void UpdateSyncProgress(CatalogSyncProgressEvent @event)
	{
		SyncProgress = @event.ProgressPercentage;
		SyncStatusMessage = Resources.DatabaseSyncProgressStatus
			.FormatMessage(@event.Kind.GetDisplayName(), @event.Added, @event.Updated, @event.Removed, @event.CompletedTasks, @event.TotalTasks);
	}
}
