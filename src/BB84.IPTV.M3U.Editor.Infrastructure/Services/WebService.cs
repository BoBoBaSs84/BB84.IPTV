// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net.Http.Json;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// The web service class.
/// </summary>
/// <param name="httpClientFactory">The http client factory instance to use.</param>
/// <param name="logger">The logger instance to use.</param>
/// <param name="eventService">The event service to publish events.</param>
internal sealed class WebService(IHttpClientFactory httpClientFactory, ILogger<WebService> logger, IEventService eventService) : IWebService
{
	public async Task<IEnumerable<BlocklistResponse>> GetBlocklistsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<BlocklistResponse>(Constants.BlocklistJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CategoryResponse>(Constants.CategoriesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<ChannelResponse>> GetChannelsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<ChannelResponse>(Constants.ChannelsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CityResponse>> GetCitiesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CityResponse>(Constants.CitiesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<CountryResponse>> GetCountriesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<CountryResponse>(Constants.CountriesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<FeedResponse>> GetFeedsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<FeedResponse>(Constants.FeedsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<GuideResponse>> GetGuidesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<GuideResponse>(Constants.GuidesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<LanguageResponse>> GetLanguagesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<LanguageResponse>(Constants.LanguagesJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<LogoResponse>> GetLogosAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<LogoResponse>(Constants.LogosJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<RegionResponse>> GetRegionsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<RegionResponse>(Constants.RegionsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<StreamResponse>> GetStreamsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<StreamResponse>(Constants.StreamsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<SubdivisionResponse>> GetSubdivisionsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<SubdivisionResponse>(Constants.SubdivisionsJsonPath, cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			ShowErrorNotification(ex);
			return [];
		}
	}

	public async Task<IEnumerable<TimezoneResponse>> GetTimezonesAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetFromApiAsync<TimezoneResponse>(Constants.TimezonesJsonPath, cancellationToken)
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

		Log.ApiRequestSent(logger, requestUrl);

		using HttpResponseMessage responseMessage = await httpClient
			.GetAsync(requestUri, cancellationToken)
			.ConfigureAwait(false);

		_ = responseMessage.EnsureSuccessStatusCode();

		IEnumerable<T>? result = await responseMessage.Content
			.ReadFromJsonAsync<IEnumerable<T>>(cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (result is not null)
		{
			Log.ApiItemsReceived(logger, result.Count(), requestUrl);
			return result;
		}
		else
		{
			Log.ApiNoItemsReceived(logger, requestUrl);
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
