// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the view model for the settings page of the IPTV M3U Editor application, providing properties
/// and commands for managing application settings such as general preferences and configurations.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase, INavigateable
{
	private readonly ISettingsService _settingsService;
	private readonly IEventService _eventService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IPathService _pathService;
	private readonly ApplicationSettings _applicationSettings;
	private bool _canLoadSettings = true;
	private bool _canSaveSettings = true;
	private IAsyncActionCommand? _loadCommand;
	private IAsyncActionCommand? _saveCommand;
	private IAsyncActionCommand? _browseDataDirectoryCommand;
	private IAsyncActionCommand? _browseLogoDirectoryCommand;
	private IAsyncActionCommand? _browseLogDirectoryCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
	/// </summary>
	/// <param name="settingsService">The service responsible for managing application settings.</param>
	/// <param name="eventService">The service responsible for managing application events.</param>
	/// <param name="fileDialogService">The service that shows the folder dialog of the data paths.</param>
	/// <param name="pathService">The service that provides the file system locations in use.</param>
	/// <param name="applicationSettings">The current application settings.</param>
	public SettingsViewModel(ISettingsService settingsService, IEventService eventService, IFileDialogService fileDialogService, IPathService pathService, ApplicationSettings applicationSettings)
	{
		_settingsService = settingsService;
		_eventService = eventService;
		_fileDialogService = fileDialogService;
		_pathService = pathService;
		_applicationSettings = applicationSettings;

		General = _applicationSettings.General;
		Database = _applicationSettings.Database;
		Logo = _applicationSettings.Logo;
		Paths = _applicationSettings.Paths;
	}

	/// <summary>
	/// Gets the general settings for the IPTV M3U Editor application, allowing users
	/// to configure options such as auto-saving and logging preferences.
	/// </summary>
	public GeneralSettings General { get; }

	/// <summary>
	/// Gets the database settings for the IPTV M3U Editor application, allowing users
	/// to configure options related to database connections and storage preferences.
	/// </summary>
	public DatabaseSettings Database { get; }

	/// <summary>
	/// Gets the logo settings for the IPTV M3U Editor application, allowing users to configure how
	/// many logos are downloaded at once and how an export writes the path of a cached logo.
	/// </summary>
	public LogoSettings Logo { get; }

	/// <summary>
	/// Gets the largest number of logos that may be downloaded at once, as the numeric input of the
	/// view takes it.
	/// </summary>
	public static decimal MaxParallelDownloadsLimit => LogoCacheRequest.MaxParallelLimit;

	/// <summary>
	/// Gets the data path settings for the IPTV M3U Editor application, allowing users to keep the
	/// database, the cached logos and the log files somewhere else.
	/// </summary>
	public PathSettings Paths { get; }

	/// <summary>
	/// Gets the directory the database is read from and written to, for as long as the application runs.
	/// </summary>
	public string DataDirectoryInUse => _pathService.DataDirectory;

	/// <summary>
	/// Gets the directory the cached logos are kept in, for as long as the application runs.
	/// </summary>
	public string LogoDirectoryInUse => _pathService.LogoDirectory;

	/// <summary>
	/// Gets the directory the log files are written to, for as long as the application runs.
	/// </summary>
	public string LogDirectoryInUse => _pathService.LogDirectory;

	/// <summary>
	/// Indicates whether the application settings can be loaded.
	/// </summary>
	public bool CanLoadSettings
	{
		get => _canLoadSettings;
		private set => SetProperty(ref _canLoadSettings, value);
	}

	/// <summary>
	/// Indicates whether the application settings can be saved.
	/// </summary>
	public bool CanSaveSettings
	{
		get => _canSaveSettings;
		private set => SetProperty(ref _canSaveSettings, value);
	}

	/// <summary>
	/// Gets the command to load the application settings.
	/// </summary>
	public IAsyncActionCommand LoadCommand
		=> _loadCommand ??= new AsyncActionCommand(LoadSettings, () => CanLoadSettings, OnError);

	/// <summary>
	/// Gets the command to save the application settings.
	/// </summary>
	public IAsyncActionCommand SaveCommand
		=> _saveCommand ??= new AsyncActionCommand(SaveSettings, () => CanSaveSettings, OnError);

	/// <summary>
	/// Gets the command that picks the directory that holds the database.
	/// </summary>
	public IAsyncActionCommand BrowseDataDirectoryCommand
		=> _browseDataDirectoryCommand ??= new AsyncActionCommand(BrowseDataDirectory, () => true, OnError);

	/// <summary>
	/// Gets the command that picks the directory that holds the cached logos.
	/// </summary>
	public IAsyncActionCommand BrowseLogoDirectoryCommand
		=> _browseLogoDirectoryCommand ??= new AsyncActionCommand(BrowseLogoDirectory, () => true, OnError);

	/// <summary>
	/// Gets the command that picks the directory that holds the log files.
	/// </summary>
	public IAsyncActionCommand BrowseLogDirectoryCommand
		=> _browseLogDirectoryCommand ??= new AsyncActionCommand(BrowseLogDirectory, () => true, OnError);

	private async Task LoadSettings()
	{
		try
		{
			CanLoadSettings = false;

			await _settingsService
				.LoadAsync()
				.ConfigureAwait(false);
		}
		finally
		{
			CanLoadSettings = true;
		}
	}

	private async Task SaveSettings()
	{
		if (GetInvalidPath() is { } invalidPath)
		{
			_eventService.Publish(new WarningOccuredEvent(Resources.DataPathIsNotValid.FormatMessage(invalidPath)));
			return;
		}

		try
		{
			CanSaveSettings = false;

			await _settingsService
				.SaveAsync(_applicationSettings)
				.ConfigureAwait(false);

			if (_pathService.HasPendingChanges())
				_eventService.Publish(new DataPathsChangedEvent());
		}
		finally
		{
			CanSaveSettings = true;
		}
	}

	private async Task BrowseDataDirectory()
	{
		if (await PickDirectoryAsync(Resources.DataDirectoryDialogTitle, Paths.DataDirectory, DataDirectoryInUse).ConfigureAwait(false) is { } directory)
			Paths.DataDirectory = directory;
	}

	private async Task BrowseLogoDirectory()
	{
		if (await PickDirectoryAsync(Resources.LogoDirectoryDialogTitle, Paths.LogoDirectory, LogoDirectoryInUse).ConfigureAwait(false) is { } directory)
			Paths.LogoDirectory = directory;
	}

	private async Task BrowseLogDirectory()
	{
		if (await PickDirectoryAsync(Resources.LogDirectoryDialogTitle, Paths.LogDirectory, LogDirectoryInUse).ConfigureAwait(false) is { } directory)
			Paths.LogDirectory = directory;
	}

	/// <summary>
	/// Shows the folder dialog, starting in the configured directory or in the one in use.
	/// </summary>
	/// <param name="title">The title of the dialog.</param>
	/// <param name="configured">The directory the setting names, which can be empty.</param>
	/// <param name="inUse">The directory the application works with.</param>
	/// <returns>The picked directory, or <see langword="null"/> if the dialog was cancelled.</returns>
	private async Task<string?> PickDirectoryAsync(string title, string configured, string inUse)
	{
		string startPath = string.IsNullOrWhiteSpace(configured) ? inUse : configured;

		return await _fileDialogService
			.ShowOpenFolderDialogAsync(title, startPath)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Reads the first configured path the file system does not take.
	/// </summary>
	/// <returns>The invalid path, or <see langword="null"/> if every path can be used.</returns>
	private string? GetInvalidPath()
	{
		string[] paths = [Paths.DataDirectory, Paths.LogoDirectory, Paths.LogDirectory];

		return paths.FirstOrDefault(path => !_pathService.IsValidDirectory(path));
	}

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.SettingsOperationFailed, exception));
}
