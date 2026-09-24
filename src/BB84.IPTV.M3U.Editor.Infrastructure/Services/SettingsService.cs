using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents a service for managing application settings, providing methods to retrieve and save settings asynchronously.
/// </summary>
internal sealed class SettingsService : ISettingsService
{
	private readonly IEventService _eventService;
	private readonly IProviderService _providerService;
	private readonly IPathService _pathService;
	private readonly ApplicationSettings _applicationSettings;
	private readonly string _settingsFilePath;

	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsService"/> class.
	/// </summary>
	/// <param name="eventService">The service responsible for managing application events.</param>
	/// <param name="providerService">The service responsible for providing access to various application resources</param>
	/// <param name="pathService">The service that provides the file system locations of the application.</param>
	/// <param name="applicationSettings">The current application settings.</param>
	public SettingsService(IEventService eventService, IProviderService providerService, IPathService pathService, ApplicationSettings applicationSettings)
	{
		_eventService = eventService;
		_providerService = providerService;
		_pathService = pathService;
		_settingsFilePath = pathService.SettingsFilePath;
		_applicationSettings = applicationSettings;

		RegisterSettingsChangeHandler();
	}

	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_providerService.File.Exists(_settingsFilePath).IsFalse())
			{
				ApplicationSettings defaultSettings = new();

				await SaveAsync(defaultSettings, cancellationToken)
					.ConfigureAwait(false);
			}

			string fileContent = await _providerService.File
				.ReadAllTextAsync(_settingsFilePath, cancellationToken)
				.ConfigureAwait(false);

			ApplicationSettings newSettings = ApplicationSettings
				.Read(fileContent);

			_applicationSettings.Load(newSettings);
			_eventService.Publish(new SettingsLoadedEvent());
		}
		catch (Exception ex)
		{
			// The notification service logs what it shows, so the message is published only.
			_eventService.Publish(new ErrorOccuredEvent(Resources.SettingsLoadFailed, ex));
		}
	}

	public async Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default)
	{
		try
		{
			string fileContent = ApplicationSettings.Write(settings);

			_providerService.Directory.CreateDirectory(_pathService.DataDirectory);

			await _providerService.File
				.WriteAllTextAsync(_settingsFilePath, fileContent, cancellationToken)
				.ConfigureAwait(false);

			_eventService.Publish(new SettingsSavedEvent());
		}
		catch (Exception ex)
		{
			_eventService.Publish(new ErrorOccuredEvent(Resources.SettingsSaveFailed, ex));
		}
	}

	private void RegisterSettingsChangeHandler()
	{
		_applicationSettings.General.PropertyChanged += (sender, args) =>
		{
			if (args.PropertyName is nameof(GeneralSettings.Language))
				_eventService.Publish(new LanguageChangedEvent(args.PropertyName));
		};
	}
}
