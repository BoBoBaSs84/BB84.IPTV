using System.Diagnostics.CodeAnalysis;
using System.Windows;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Presentation.Properties;

namespace BB84.IPTV.M3U.Editor.Presentation.Services;

/// <summary>
/// Represents a service for displaying notifications to the user.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class is just an abstraction for the MessageBox.")]
internal sealed class NotificationService : INotificationService
{
	private readonly IEventService _eventService;

	/// <summary>
	/// Initializes a new instance of the <see cref="NotificationService"/> class.
	/// </summary>
	/// <param name="eventService">The event service to subscribe to for showing notifications.</param>
	public NotificationService(IEventService eventService)
	{
		_eventService = eventService;
		RegisterEventHandlers();
	}

	public void ShowError(string message)
		=> DisplayMessage(message, Resources.ErrorMessageCaptition, MessageBoxImage.Error);

	public void ShowInformation(string message)
		=> DisplayMessage(message, Resources.InformationMessageCaptition, MessageBoxImage.Information);

	public void ShowWarning(string message)
		=> DisplayMessage(message, Resources.WarningMessageCaptition, MessageBoxImage.Warning);

	public NotificationResult ShowQuestion(string message)
		=> ToNotificationResult(DisplayQuestion(message, Resources.QuestionMessageCaptition, MessageBoxButton.YesNo, MessageBoxImage.Question));

	public NotificationResult ShowRetry(string message)
		=> ToNotificationResult(MessageBox.Show(message, Resources.RetryMessageCaptition, MessageBoxButton.OKCancel, MessageBoxImage.Question));

	private static void DisplayMessage(string message, string caption, MessageBoxImage icon)
		=> MessageBox.Show(message, caption, MessageBoxButton.OK, icon);

	private static MessageBoxResult DisplayQuestion(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
		=> MessageBox.Show(message, caption, buttons, icon);

	private static NotificationResult ToNotificationResult(MessageBoxResult result) => result switch
	{
		MessageBoxResult.OK => NotificationResult.OK,
		MessageBoxResult.Cancel => NotificationResult.Cancel,
		MessageBoxResult.Yes => NotificationResult.Yes,
		MessageBoxResult.No => NotificationResult.No,
		_ => NotificationResult.None
	};

	private void RegisterEventHandlers()
	{
		_eventService.Subscribe<ErrorOccuredEvent>(e => ShowError(e.Message));
		_eventService.Subscribe<InformationOccuredEvent>(e => ShowInformation(e.Message));
		_eventService.Subscribe<WarningOccuredEvent>(e => ShowWarning(e.Message));
	}
}
