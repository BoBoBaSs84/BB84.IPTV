using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
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

	private static readonly Action<ILogger, string, Exception?> LogError =
		LoggerMessage.Define<string>(LogLevel.Error, 0, "{Error}");

	public async Task<IPlaylist?> LoadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		string message;
		try
		{
			message = $"Loading playlist from file: {filePath}";
			loggerService.Log(LogInformation, message);
			PublishDelayedStatus(message);

			string[] fileLines = await providerService.File
				.ReadAllLinesAsync(filePath, cancellationToken)
				.ConfigureAwait(false);

			IPlaylist playlist = serializer.Deserialize(fileLines);

			message = $"Successfully loaded playlist from file: {filePath}";
			loggerService.Log(LogInformation, message);
			PublishDelayedStatus(message);

			return playlist;
		}
		catch (Exception ex)
		{
			message = $"Failed to load playlist from file: {filePath}";
			loggerService.Log(LogError, message, ex);
			PublishErrorNotification(message);
			return null;
		}
	}

	public async Task Save(IPlaylist playlist, string filePath, CancellationToken cancellationToken = default)
	{
		string message;
		try
		{
			message = $"Saving playlist to file: {filePath}";
			loggerService.Log(LogInformation, message);
			PublishDelayedStatus(message);

			string fileContent = serializer.Serialize(playlist);

			await providerService.File
				.WriteAllTextAsync(filePath, fileContent, cancellationToken)
				.ConfigureAwait(false);

			message = $"Successfully saved playlist to file: {filePath}";
			loggerService.Log(LogInformation, message);
			PublishDelayedStatus(message);
		}
		catch (Exception ex)
		{
			message = $"Failed to save playlist to file: {filePath}";
			loggerService.Log(LogError, message, ex);
			PublishErrorNotification(message);
		}
	}

	private void PublishDelayedStatus(string message)
		=> eventService.Publish(new DelayedStatusChangedEvent(message));

	private void PublishErrorNotification(string message)
		=> eventService.Publish(new ErrorOccuredEvent(message));
}
