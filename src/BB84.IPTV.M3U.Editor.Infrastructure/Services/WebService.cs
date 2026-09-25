// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net.Http.Json;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// The web service class.
/// </summary>
/// <param name="httpClientFactory">The http client factory instance to use.</param>
/// <param name="loggerService">The logger service instance to use.</param>
/// <param name="eventService">The event service to publish events.</param>
internal sealed class WebService(IHttpClientFactory httpClientFactory, ILoggerService<WebService> loggerService, IEventService eventService) : IWebService
{
	private static readonly Action<ILogger, string, Exception?> LogInformation =
		LoggerMessage.Define<string>(LogLevel.Information, 1, "{Information}");

	public async Task<IEnumerable<BlocklistRequest>> GetBlocklistsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<BlocklistRequest>(Constants.BlocklistJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CategoryRequest>> GetCategoriesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CategoryRequest>(Constants.CategoriesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<ChannelRequest>> GetChannelsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<ChannelRequest>(Constants.ChannelsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CityRequest>> GetCitiesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CityRequest>(Constants.CitiesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CountryRequest>> GetCountriesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CountryRequest>(Constants.CountriesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<FeedRequest>> GetFeedsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<FeedRequest>(Constants.FeedsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<GuideRequest>> GetGuidesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<GuideRequest>(Constants.GuidesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<LanguageRequest>> GetLanguagesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<LanguageRequest>(Constants.LanguagesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<LogoRequest>> GetLogosAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<LogoRequest>(Constants.LogosJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<RegionRequest>> GetRegionsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<RegionRequest>(Constants.RegionsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<StreamRequest>> GetStreamsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<StreamRequest>(Constants.StreamsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<SubdivisionRequest>> GetSubdivisionsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<SubdivisionRequest>(Constants.SubdivisionsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<TimezoneRequest>> GetTimezonesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<TimezoneRequest>(Constants.TimezonesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	private async Task<IEnumerable<T>> GetFromApiAsync<T>(string requestUri, CancellationToken cancellationToken = default)
	{
		HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);

		string requestUrl = $"{httpClient.BaseAddress}/{requestUri}";

		loggerService.Log(LogInformation, $"Sending request to '{requestUrl}'");

		using HttpResponseMessage responseMessage = await httpClient
			.GetAsync(requestUri, cancellationToken)
			.ConfigureAwait(false);

		_ = responseMessage.EnsureSuccessStatusCode();

		IEnumerable<T>? result = await responseMessage.Content
			.ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (result is not null)
		{
			loggerService.Log(LogInformation, $"Received {result.Count()} items from '{requestUrl}'");
			return result;
		}
		else
		{
			loggerService.Log(LogInformation, $"Received no items from '{requestUrl}'");
			return [];
		}
	}

	/// <summary>
	/// Reports a failed request, the notification service logs what it shows.
	/// </summary>
	/// <param name="exception">The exception the request failed with.</param>
	private void ShowErrorNotification(Exception exception)
		=> eventService.Publish(new ErrorOccuredEvent(Resources.WebRequestFailed, exception));
}
