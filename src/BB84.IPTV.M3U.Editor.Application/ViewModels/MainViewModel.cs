using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

using Microsoft.Extensions.Hosting;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the main view model of the application.
/// </summary>
public sealed class MainViewModel : ViewModelBase, INavigateable
{
	private readonly IEventService _eventService;
	private readonly IHostEnvironment _hostEnvironment;
	private readonly INotificationService _notificationService;
	private readonly IUserService _userService;
	private readonly SynchronizationContext? _synchronizationContext;
	private IActionCommand? _showAboutControl;
	private IAsyncActionCommand? _exitApplicationCommand;
	private IActionCommand? _openSettingsCommand;
	private string _applicationTitle;
	private string _statusText;
	private int _progressBarValue;
	private bool _progressBarVisible;
	private int _progressBarMaximum = 100;
	private int _progressBarMinimum;


	/// <summary>
	/// Initializes a new instance of the <see cref="MainViewModel"/> class.
	/// </summary>
	/// <param name="eventService">The event service instance to use.</param>
	/// <param name="hostEnvironment">The host environment instance to use.</param>
	/// <param name="notificationService">The notification service instance to use.</param>
	/// <param name="userService">The user service instance to use.</param>
	/// <param name="navigationService">The navigation service instance to use.</param>
	public MainViewModel(IEventService eventService, IHostEnvironment hostEnvironment, INotificationService notificationService, IUserService userService, INavigationService navigationService)
	{
		_eventService = eventService;
		_hostEnvironment = hostEnvironment;
		_notificationService = notificationService;
		_userService = userService;
		_synchronizationContext = SynchronizationContext.Current;
		_applicationTitle = $"{_hostEnvironment.ApplicationName} - {_hostEnvironment.EnvironmentName}";
		_statusText = string.Empty;

		NavigationService = navigationService;

		_eventService.Subscribe<DelayedStatusChangedEvent>(OnStatusChanged);
		_eventService.Subscribe<ProgressChangedEvent>(OnProgressChanged);
		_eventService.Subscribe<LanguageChangedEvent>(OnLanguageChanged);
	}

	/// <summary>
	/// The application title for the main window.
	/// </summary>
	public string ApplicationTitle
	{
		get => _applicationTitle;
		private set => SetProperty(ref _applicationTitle, value);
	}

	/// <summary>
	/// The current user.
	/// </summary>
	public string CurrentUser
		=> $"{_userService.Domain}\\{_userService.Name}@{_userService.Machine}";

	/// <summary>
	/// Gets or sets the status text for the main window.
	/// </summary>
	public string StatusText
	{
		get => _statusText;
		private set => SetProperty(ref _statusText, value);
	}

	/// <summary>
	/// Gets or sets the value of the progress bar.
	/// </summary>
	public int ProgressBarValue
	{
		get => _progressBarValue;
		set => SetProperty(ref _progressBarValue, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the progress bar is visible.
	/// </summary>
	public bool ProgressBarVisible
	{
		get => _progressBarVisible;
		set => SetProperty(ref _progressBarVisible, value);
	}

	/// <summary>
	/// Gets or sets the maximum value of the progress bar.
	/// </summary>
	public int ProgressBarMaximum
	{
		get => _progressBarMaximum;
		set => SetProperty(ref _progressBarMaximum, value);
	}

	/// <summary>
	/// Gets or sets the minimum value of the progress bar.
	/// </summary>
	public int ProgressBarMinimum
	{
		get => _progressBarMinimum;
		set => SetProperty(ref _progressBarMinimum, value);
	}

	/// <summary>
	/// Gets the navigation service for navigating between view models.
	/// </summary>
	public INavigationService NavigationService { get; }

	/// <summary>
	/// Gets the command to show the about control.
	/// </summary>
	public IActionCommand ShowAboutControl
		=> _showAboutControl ??= new ActionCommand(NavigationService.NavigateTo<AboutViewModel>);

	/// <summary>
	/// Gets the command to exit the application.
	/// </summary>
	public IAsyncActionCommand ExitApplicationCommand
		=> _exitApplicationCommand ??= new AsyncActionCommand(ExitApplicationAsync);

	private async Task ExitApplicationAsync()
	{
		NotificationResult result = await _notificationService
			.ShowQuestionAsync(Resources.ExitApplicationQuestion)
			.ConfigureAwait(true);

		if (result == NotificationResult.Yes)
			_eventService.Publish(new ExitRequestedEvent());
	}

	/// <summary>
	/// The command to open the settings control.
	/// </summary>
	public IActionCommand OpenSettingsCommand
		=> _openSettingsCommand ??= new ActionCommand(NavigationService.NavigateTo<SettingsViewModel>);

	private void OnStatusChanged(DelayedStatusChangedEvent @event)
	{
		if (_synchronizationContext is not null)
			_synchronizationContext.Post(_ => ChangeStatus(@event), null);
		else
			ChangeStatus(@event);
	}

	private void ChangeStatus(DelayedStatusChangedEvent @event)
	{
		StatusText = @event.Text;

		if (@event.AutoClear)
		{
			Task.Delay(@event.Duration)
				.ContinueWith(_ => StatusText = string.Empty);
		}
	}

	private void OnProgressChanged(ProgressChangedEvent @event)
	{
		if (_synchronizationContext is not null)
			_synchronizationContext.Post(_ => ChangeProgress(@event), null);
		else
			ChangeProgress(@event);
	}

	private void ChangeProgress(ProgressChangedEvent @event)
	{
		ProgressBarMaximum = @event.Maximum;
		ProgressBarMinimum = @event.Minimum;
		ProgressBarValue = @event.Value;
		ProgressBarVisible = @event.Value is > 0 and < 100;
	}

	private async void OnLanguageChanged(LanguageChangedEvent @event)
	{
		NotificationResult result = await _notificationService
			.ShowQuestionAsync(Resources.ChangedLanguageRestartApplicationQuestion)
			.ConfigureAwait(true);

		if (result == NotificationResult.Yes)
			_eventService.Publish(new RestartRequestedEvent());
	}
}
