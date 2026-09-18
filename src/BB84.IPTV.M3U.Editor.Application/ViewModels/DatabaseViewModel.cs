using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the view model for managing database operations such as checking, creating, and importing the database.
/// </summary>
public sealed class DatabaseViewModel : ViewModelBase, INavigateable
{
	private readonly IEventService _eventService;
	private readonly IDatabaseService _databaseService;
	private readonly SynchronizationContext? _synchronizationContext;
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

	/// <summary>
	/// Initializes a new instance of the <see cref="DatabaseViewModel"/> class.
	/// </summary>
	/// <param name="eventService">The event service for subscribing and publishing events.</param>
	/// <param name="databaseService">The database service for managing database operations.</param>
	public DatabaseViewModel(IEventService eventService, IDatabaseService databaseService)
	{
		_eventService = eventService;
		_databaseService = databaseService;
		_synchronizationContext = SynchronizationContext.Current;

		_eventService.Subscribe<DatabaseImportProgressEvent>(OnDatabaseImportProgress);
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
		=> _eventService.Publish(new ErrorOccuredEvent("Failed to check database.", exception));

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
		=> _eventService.Publish(new ErrorOccuredEvent("Failed to create database.", exception));

	private async Task ImportDatabaseAsync()
	{
		try
		{
			DatabaseImporting = true;
			ImportProgress = 0;
			ImportStatusMessage = "Starting database import...";

			DatabaseImportResponse response = await _databaseService
				.ImportDatabaseAsync();

			DatabaseImported = response.IsSuccess;
			ImportStatusMessage = response.IsSuccess
				? $"Import completed successfully. Total records: {response.TotalImported}"
				: "Import completed with no records.";
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
		_eventService.Publish(new ErrorOccuredEvent("Failed to import database.", exception));
		ImportStatusMessage = "Import failed.";
		ImportProgress = 0;
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
		ImportStatusMessage = $"Importing {@event.RepositoryName}: {@event.RecordsImported} records ({@event.CompletedTasks}/{@event.TotalTasks})";
	}
}
