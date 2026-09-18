using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
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
	private readonly ApplicationSettings _applicationSettings;
	private bool _canLoadSettings = true;
	private bool _canSaveSettings = true;
	private IAsyncActionCommand? _loadCommand;
	private IAsyncActionCommand? _saveCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
	/// </summary>
	/// <param name="settingsService">The service responsible for managing application settings.</param>
	/// <param name="eventService">The service responsible for managing application events.</param>
	/// <param name="applicationSettings">The current application settings.</param>
	public SettingsViewModel(ISettingsService settingsService, IEventService eventService, ApplicationSettings applicationSettings)
	{
		_settingsService = settingsService;
		_eventService = eventService;
		_applicationSettings = applicationSettings;

		General = _applicationSettings.General;
		Database = _applicationSettings.Database;
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
		try
		{
			CanSaveSettings = false;

			await _settingsService
				.SaveAsync(_applicationSettings)
				.ConfigureAwait(false);
		}
		finally
		{
			CanSaveSettings = true;
		}
	}

	private void OnError(Exception exception)
	{
		ErrorOccuredEvent @event = new("An error occurred while managing application settings.", exception);
		_eventService.Publish(@event);
	}
}
