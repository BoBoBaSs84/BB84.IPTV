using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;
using Avalonia.Threading;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Extensions;
using BB84.IPTV.M3U.Editor.Properties;
using BB84.IPTV.M3U.Editor.Views;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// Represents a service for displaying notifications to the user.
/// </summary>
/// <remarks>
/// Every notification the application publishes is shown here and written to the log, so a
/// publisher does not have to log what it reports.
/// </remarks>
[ExcludeFromCodeCoverage(Justification = "This class is just an abstraction for the message dialog.")]
internal sealed class NotificationService : INotificationService
{
	private readonly IEventService _eventService;
	private readonly ILoggerService<NotificationService> _loggerService;

	private static readonly Action<ILogger, string, Exception?> LogError =
		LoggerMessage.Define<string>(LogLevel.Error, 0, "{Error}");

	private static readonly Action<ILogger, string, Exception?> LogWarning =
		LoggerMessage.Define<string>(LogLevel.Warning, 0, "{Warning}");

	private static readonly Action<ILogger, string, Exception?> LogInformation =
		LoggerMessage.Define<string>(LogLevel.Information, 0, "{Information}");

	/// <summary>
	/// Initializes a new instance of the <see cref="NotificationService"/> class.
	/// </summary>
	/// <param name="eventService">The event service to subscribe to for showing notifications.</param>
	/// <param name="loggerService">The logger service that writes what is shown to the user.</param>
	public NotificationService(IEventService eventService, ILoggerService<NotificationService> loggerService)
	{
		_eventService = eventService;
		_loggerService = loggerService;
		RegisterEventHandlers();
	}

	public Task ShowErrorAsync(string message)
		=> ShowAsync(message, Resources.ErrorMessageCaptition, MessageDialogKind.Error, NotificationResult.OK);

	public Task ShowInformationAsync(string message)
		=> ShowAsync(message, Resources.InformationMessageCaptition, MessageDialogKind.Information, NotificationResult.OK);

	public Task ShowWarningAsync(string message)
		=> ShowAsync(message, Resources.WarningMessageCaptition, MessageDialogKind.Warning, NotificationResult.OK);

	public Task<NotificationResult> ShowQuestionAsync(string message)
		=> ShowAsync(message, Resources.QuestionMessageCaptition, MessageDialogKind.Question, NotificationResult.Yes, NotificationResult.No);

	public Task<NotificationResult> ShowRetryAsync(string message)
		=> ShowAsync(message, Resources.RetryMessageCaptition, MessageDialogKind.Question, NotificationResult.OK, NotificationResult.Cancel);

	/// <summary>
	/// Shows the message on the UI thread, so it can be called from background threads as well.
	/// </summary>
	private static Task<NotificationResult> ShowAsync(string message, string caption, MessageDialogKind kind, params NotificationResult[] buttons)
		=> Dispatcher.UIThread.InvokeAsync(() => ShowDialogAsync(new MessageDialog(message, caption, kind, buttons)));

	private static async Task<NotificationResult> ShowDialogAsync(MessageDialog dialog)
	{
		Window? owner = ApplicationExtensions.GetActiveWindow();

		if (owner is { IsVisible: true })
		{
			await dialog.ShowDialog(owner).ConfigureAwait(true);
			return dialog.Result;
		}

		// No window to be modal to yet, e.g. an error during startup.
		TaskCompletionSource<NotificationResult> completionSource = new();
		dialog.Closed += (s, e) => completionSource.TrySetResult(dialog.Result);
		dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		dialog.Show();

		return await completionSource.Task.ConfigureAwait(true);
	}

	private void RegisterEventHandlers()
	{
		_eventService.Subscribe<ErrorOccuredEvent>(e =>
		{
			_loggerService.Log(LogError, e.Message, e.Exception);
			_ = ShowErrorAsync(e.Message);
		});

		_eventService.Subscribe<InformationOccuredEvent>(e =>
		{
			_loggerService.Log(LogInformation, e.Message);
			_ = ShowInformationAsync(e.Message);
		});

		_eventService.Subscribe<WarningOccuredEvent>(e =>
		{
			_loggerService.Log(LogWarning, e.Message);
			_ = ShowWarningAsync(e.Message);
		});
	}
}