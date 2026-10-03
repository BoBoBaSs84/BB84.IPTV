// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that keeps the catalog from growing old without the user noticing.
/// </summary>
/// <param name="databaseService">The service that reads and synchronizes the catalog.</param>
/// <param name="notificationService">The service that asks the user.</param>
/// <param name="providerService">The provider service, which supplies the clock.</param>
/// <param name="eventService">The event service, for reporting a failure.</param>
/// <param name="applicationSettings">The settings, which say when and whether to update.</param>
internal sealed class CatalogUpdateService(
	IDatabaseService databaseService,
	INotificationService notificationService,
	IProviderService providerService,
	IEventService eventService,
	ApplicationSettings applicationSettings) : ICatalogUpdateService
{
	public async Task<bool> RunStartupCheckAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			int interval = applicationSettings.Database.UpdateIntervalDays;

			if (interval <= 0)
				return false;

			IReadOnlyList<CatalogStatusResponse> statuses = await databaseService
				.GetCatalogStatusAsync(cancellationToken)
				.ConfigureAwait(true);

			int? age = GetAgeInDays(statuses, providerService.DateTime.UtcNow);

			// A catalog that was never read is as due as one that is older than the interval.
			if (age.HasValue && age.Value < interval)
				return false;

			if (!applicationSettings.Database.AutoUpdate && !await AskAsync(age).ConfigureAwait(true))
				return false;

			// The diff of the whole catalog runs in memory, so it stays off the thread of the window.
			_ = await Task.Run(() => databaseService.SynchronizeAsync(cancellationToken), cancellationToken)
				.ConfigureAwait(true);

			return true;
		}
		catch (OperationCanceledException)
		{
			// The application is closing, the next start checks again.
			return false;
		}
		catch (Exception exception)
		{
			eventService.Publish(new ErrorOccuredEvent(Resources.DatabaseSyncFailed, exception));
			return false;
		}
	}

	/// <summary>
	/// Asks whether the catalog is updated now.
	/// </summary>
	private async Task<bool> AskAsync(int? age)
	{
		string question = age.HasValue
			? Resources.DatabaseSyncQuestion.FormatMessage(age.Value)
			: Resources.DatabaseSyncNeverQuestion;

		NotificationResult result = await notificationService
			.ShowQuestionAsync(question)
			.ConfigureAwait(true);

		return result is NotificationResult.Yes;
	}

	/// <summary>
	/// Gets how many days ago the catalog was read, counted from the list that was read last of
	/// all, or <see langword="null"/> if a list was never read.
	/// </summary>
	private static int? GetAgeInDays(IReadOnlyList<CatalogStatusResponse> statuses, DateTime now)
	{
		if (statuses.Count is 0 || statuses.Any(status => !status.IsImported))
			return null;

		DateTime oldest = statuses.Min(status => status.LastChecked!.Value);

		return (int)Math.Floor((now - oldest).TotalDays);
	}
}
