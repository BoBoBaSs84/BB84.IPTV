// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net.Http.Json;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// The web service class.
/// </summary>
/// <param name="httpClientFactory">The http client factory instance to use.</param>
/// <param name="logger">The logger instance to use.</param>
/// <remarks>
/// A failed request throws, so the caller stops at the first failure and reports it once.
/// </remarks>
internal sealed class WebService(IHttpClientFactory httpClientFactory, ILogger<WebService> logger) : IWebService
{
	public Task<IEnumerable<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<CategoryResponse>(Constants.CategoriesJsonPath, cancellationToken);

	public Task<IEnumerable<ChannelResponse>> GetChannelsAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<ChannelResponse>(Constants.ChannelsJsonPath, cancellationToken);

	public Task<IEnumerable<CountryResponse>> GetCountriesAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<CountryResponse>(Constants.CountriesJsonPath, cancellationToken);

	public Task<IEnumerable<FeedResponse>> GetFeedsAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<FeedResponse>(Constants.FeedsJsonPath, cancellationToken);

	public Task<IEnumerable<GuideResponse>> GetGuidesAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<GuideResponse>(Constants.GuidesJsonPath, cancellationToken);

	public Task<IEnumerable<LanguageResponse>> GetLanguagesAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<LanguageResponse>(Constants.LanguagesJsonPath, cancellationToken);

	public Task<IEnumerable<LogoResponse>> GetLogosAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<LogoResponse>(Constants.LogosJsonPath, cancellationToken);

	public Task<IEnumerable<StreamResponse>> GetStreamsAsync(CancellationToken cancellationToken = default)
		=> GetFromApiAsync<StreamResponse>(Constants.StreamsJsonPath, cancellationToken);

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
}
