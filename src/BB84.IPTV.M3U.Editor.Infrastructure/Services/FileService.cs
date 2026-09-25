// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the file service for loading and saving playlists.
/// </summary>
/// <param name="eventService">The event service instance to use for publishing events.</param>
/// <param name="loggerService">The logger service instance to use for logging.</param>
/// <param name="providerService">The provider service instance to use for file operations.</param>
/// <param name="serializer">The serializer service instance to use for playlist serialization and deserialization.</param>
internal sealed class FileService(IEventService eventService, ILoggerService<FileService> loggerService, IProviderService providerService, ISerializerService serializer) : IFileService
{
	private static readonly Action<ILogger, string, Exception?> LogInformation =
		LoggerMessage.Define<string>(LogLevel.Information, 0, "{Information}");

	public async Task<IPlaylist?> LoadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		try
		{
			loggerService.Log(LogInformation, $"Loading playlist from file: {filePath}");
			PublishDelayedStatus(Resources.PlaylistFileLoading.FormatMessage(filePath));

			string[] fileLines = await providerService.File
				.ReadAllLinesAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			IPlaylist playlist = serializer.Deserialize(fileLines);

			loggerService.Log(LogInformation, $"Successfully loaded playlist from file: {filePath}");
			PublishDelayedStatus(Resources.PlaylistFileLoaded.FormatMessage(filePath));

			return playlist;
		}
		catch (Exception ex)
		{
			// The notification service logs what it shows, so the message is published only.
			PublishErrorNotification(Resources.PlaylistFileLoadFailed.FormatMessage(filePath), ex);
			return null;
		}
	}

	public async Task Save(IPlaylist playlist, string filePath, CancellationToken cancellationToken = default)
	{
		try
		{
			loggerService.Log(LogInformation, $"Saving playlist to file: {filePath}");
			PublishDelayedStatus(Resources.PlaylistFileSaving.FormatMessage(filePath));

			string fileContent = serializer.Serialize(playlist);

			await providerService.File
				.WriteAllTextAsync(filePath, fileContent, cancellationToken)
				.ConfigureAwait(false);

			loggerService.Log(LogInformation, $"Successfully saved playlist to file: {filePath}");
			PublishDelayedStatus(Resources.PlaylistFileSaved.FormatMessage(filePath));
		}
		catch (Exception ex)
		{
			PublishErrorNotification(Resources.PlaylistFileSaveFailed.FormatMessage(filePath), ex);
		}
	}

	private void PublishDelayedStatus(string message)
		=> eventService.Publish(new DelayedStatusChangedEvent(message));

	private void PublishErrorNotification(string message, Exception exception)
		=> eventService.Publish(new ErrorOccuredEvent(message, exception));
}
