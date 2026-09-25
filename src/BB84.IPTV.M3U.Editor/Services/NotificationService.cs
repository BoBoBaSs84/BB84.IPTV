// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;
using Avalonia.Threading;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
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
	private readonly ILogger<NotificationService> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="NotificationService"/> class.
	/// </summary>
	/// <param name="eventService">The event service to subscribe to for showing notifications.</param>
	/// <param name="logger">The logger that writes what is shown to the user.</param>
	public NotificationService(IEventService eventService, ILogger<NotificationService> logger)
	{
		_eventService = eventService;
		_logger = logger;
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
			Log.NotificationError(_logger, e.Message, e.Exception);
			_ = ShowErrorAsync(e.Message);
		});

		_eventService.Subscribe<InformationOccuredEvent>(e =>
		{
			Log.NotificationInformation(_logger, e.Message);
			_ = ShowInformationAsync(e.Message);
		});

		_eventService.Subscribe<WarningOccuredEvent>(e =>
		{
			Log.NotificationWarning(_logger, e.Message);
			_ = ShowWarningAsync(e.Message);
		});
	}
}