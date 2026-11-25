// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents a web service interface for fetching IPTV-related data.
/// </summary>
public interface IWebService
{
	/// <summary>
	/// Gets the categories from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of category responses.
	/// </returns>
	Task<IEnumerable<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the channels from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of channel responses.
	/// </returns>
	Task<IEnumerable<ChannelResponse>> GetChannelsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the countries from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of country responses.
	/// </returns>
	Task<IEnumerable<CountryResponse>> GetCountriesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the feeds from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of feed responses.
	/// </returns>
	Task<IEnumerable<FeedResponse>> GetFeedsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the guides from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of guide responses.
	/// </returns>
	Task<IEnumerable<GuideResponse>> GetGuidesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the languages from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of language responses.
	/// </returns>
	Task<IEnumerable<LanguageResponse>> GetLanguagesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the logos from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of logo responses.
	/// </returns>
	Task<IEnumerable<LogoResponse>> GetLogosAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the streams from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of stream responses.
	/// </returns>
	Task<IEnumerable<StreamResponse>> GetStreamsAsync(CancellationToken cancellationToken = default);
}
