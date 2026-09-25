// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

/// <summary>
/// The interface for the notification service.
/// </summary>
public interface INotificationService
{
	/// <summary>
	/// Shows a error message.
	/// </summary>
	/// <param name="message">The message to show.</param>
	/// <returns>A task that completes when the message has been closed.</returns>
	Task ShowErrorAsync(string message);

	/// <summary>
	/// Shows a informational message.
	/// </summary>
	/// <param name="message">The message to show.</param>
	/// <returns>A task that completes when the message has been closed.</returns>
	Task ShowInformationAsync(string message);

	/// <summary>
	/// Shows a warning message.
	/// </summary>
	/// <param name="message">The message to show.</param>
	/// <returns>A task that completes when the message has been closed.</returns>
	Task ShowWarningAsync(string message);

	/// <summary>
	/// Shows a retry message and return the result.
	/// </summary>
	/// <param name="message">The message to show.</param>
	/// <returns>The result of the retry message.</returns>
	Task<NotificationResult> ShowRetryAsync(string message);

	/// <summary>
	/// Shows a question message and return the result.
	/// </summary>
	/// <param name="message">The question to show.</param>
	/// <returns>The result of the question.</returns>
	Task<NotificationResult> ShowQuestionAsync(string message);
}