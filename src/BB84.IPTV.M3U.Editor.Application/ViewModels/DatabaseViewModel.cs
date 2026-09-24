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
	private readonly SynchronizationContext? _synchronizationContext;
	private CancellationTokenSource? _logoCacheCancellation;
	private LogoCacheStatusResponse _logoCacheStatus = new();
	private bool _logosCaching;
	private int _logoProgress;
	private string _logoStatusMessage = string.Empty;
	private bool _databaseChecked;
	private bool _databaseChecking;
	private bool _databaseCreated;
	private bool _databaseCreating;
	private bool _databaseImported;
	private bool _databaseImporting;
	private int _importProgress;
	private int _importProgressMaximum = 100;
	private string _importStatusMessage = string.Empty;
	private IAsyncActionCommand? _checkDatabaseCommand;
	private IAsyncActionCommand? _createDatabaseCommand;
	private IAsyncActionCommand? _importDatabaseCommand;
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
		_synchronizationContext = SynchronizationContext.Current;

		_eventService.Subscribe<DatabaseImportProgressEvent>(OnDatabaseImportProgress);
		_eventService.Subscribe<LogoCacheProgressEvent>(OnLogoCacheProgress);

		// The commands depend on the database state, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
	}

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
	/// Indicates whether the database has been imported or not.
	/// </summary>
	public bool DatabaseImported
	{
		get => _databaseImported;
		private set => SetProperty(ref _databaseImported, value);
	}

	/// <summary>
	/// Indicates whether the database is being imported or not.
	/// </summary>
	public bool DatabaseImporting
	{
		get => _databaseImporting;
		private set => SetProperty(ref _databaseImporting, value);
	}

	/// <summary>
	/// Gets the current import progress value (0-100).
	/// </summary>
	public int ImportProgress
	{
		get => _importProgress;
		private set => SetProperty(ref _importProgress, value);
	}

	/// <summary>
	/// Gets the maximum value for the import progress (typically 100).
	/// </summary>
	public int ImportProgressMaximum
	{
		get => _importProgressMaximum;
		private set => SetProperty(ref _importProgressMaximum, value);
	}

	/// <summary>
	/// Gets the current import status message indicating which repository is being imported.
	/// </summary>
	public string ImportStatusMessage
	{
		get => _importStatusMessage;
		private set => SetProperty(ref _importStatusMessage, value);
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
	/// The command to import the database.
	/// </summary>
	public IAsyncActionCommand ImportDatabaseCommand
		=> _importDatabaseCommand ??= new AsyncActionCommand(ImportDatabaseAsync, CanImportDatabase, ImportDatabaseFailed);

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

	private async Task ImportDatabaseAsync()
	{
		try
		{
			DatabaseImporting = true;
			ImportProgress = 0;
			ImportStatusMessage = Resources.DatabaseImportStarted;

			DatabaseImportResponse response = await _databaseService
				.ImportDatabaseAsync();

			DatabaseImported = response.IsSuccess;
			ImportStatusMessage = response.IsSuccess
				? Resources.DatabaseImportSucceeded.FormatMessage(response.TotalImported)
				: Resources.DatabaseImportWithoutRecords;
		}
		finally
		{
			DatabaseImporting = false;
		}
	}

	private bool CanImportDatabase()
		=> _databaseImported.IsFalse() && _databaseImporting.IsFalse() && _databaseCreated.IsTrue();

	private void ImportDatabaseFailed(Exception exception)
	{
		_eventService.Publish(new ErrorOccuredEvent(Resources.DatabaseImportFailed, exception));
		ImportStatusMessage = Resources.DatabaseImportFailed;
		ImportProgress = 0;
	}

	private void RaiseCommandStatesChanged()
	{
		_checkDatabaseCommand?.RaiseCanExecuteChanged();
		_createDatabaseCommand?.RaiseCanExecuteChanged();
		_importDatabaseCommand?.RaiseCanExecuteChanged();
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
		_logoCacheCancellation?.Dispose();
		_logoCacheCancellation = new CancellationTokenSource();

		try
		{
			LogosCaching = true;
			LogoProgress = 0;

			int downloaded = await _logoService
				.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = _settings.Logo.MaxParallelDownloads }, _logoCacheCancellation.Token)
				.ConfigureAwait(true);

			LogoStatusMessage = Resources.LogoCacheDownloaded.FormatMessage(downloaded);
		}
		finally
		{
			LogosCaching = false;
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
		LogoStatusMessage = Resources.LogoCacheFailed;
		LogoProgress = 0;
	}

	private void OnLogoCacheProgress(LogoCacheProgressEvent @event)
	{
		if (_synchronizationContext is not null)
			_synchronizationContext.Post(_ => UpdateLogoProgress(@event), null);
		else
			UpdateLogoProgress(@event);
	}

	private void UpdateLogoProgress(LogoCacheProgressEvent @event)
	{
		LogoProgress = @event.ProgressPercentage;
		LogoStatusMessage = Resources.LogoCacheProgress.FormatMessage(@event.ProcessedCount, @event.TotalCount);
	}

	private void OnDatabaseImportProgress(DatabaseImportProgressEvent @event)
	{
		if (_synchronizationContext is not null)
		{
			_synchronizationContext.Post(_ => UpdateImportProgress(@event), null);
		}
		else
		{
			UpdateImportProgress(@event);
		}
	}

	private void UpdateImportProgress(DatabaseImportProgressEvent @event)
	{
		ImportProgress = @event.ProgressPercentage;
		ImportStatusMessage = Resources.DatabaseImportProgressStatus
			.FormatMessage(@event.RepositoryName, @event.RecordsImported, @event.CompletedTasks, @event.TotalTasks);
	}
}
