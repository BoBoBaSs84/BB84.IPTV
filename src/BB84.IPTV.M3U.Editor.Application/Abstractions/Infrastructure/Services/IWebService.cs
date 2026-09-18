using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents a web service interface for fetching IPTV-related data.
/// </summary>
public interface IWebService
{
	/// <summary>
	/// Gets the blocklists from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of blocklist requests.
	/// </returns>
	Task<IEnumerable<BlocklistRequest>> GetBlocklistsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the categories from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of category requests.
	/// </returns>
	Task<IEnumerable<CategoryRequest>> GetCategoriesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the channels from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of channel requests.
	/// </returns>
	Task<IEnumerable<ChannelRequest>> GetChannelsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the cities from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of city requests.
	/// </returns>
	Task<IEnumerable<CityRequest>> GetCitiesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the countries from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of country requests.
	/// </returns>
	Task<IEnumerable<CountryRequest>> GetCountriesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the feeds from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of feed requests.
	/// </returns>
	Task<IEnumerable<FeedRequest>> GetFeedsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the guides from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of guide requests.
	/// </returns>
	Task<IEnumerable<GuideRequest>> GetGuidesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the languages from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of language requests.
	/// </returns>
	Task<IEnumerable<LanguageRequest>> GetLanguagesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the logos from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of logo requests.
	/// </returns>
	Task<IEnumerable<LogoRequest>> GetLogosAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the regions from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of region requests.
	/// </returns>
	Task<IEnumerable<RegionRequest>> GetRegionsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the streams from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of stream requests.
	/// </returns>
	Task<IEnumerable<StreamRequest>> GetStreamsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the sub-divions from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of sub-division requests.
	/// </returns>
	Task<IEnumerable<SubdivisionRequest>> GetSubdivisionsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the timezones from the web service.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains a collection of timezone requests.
	/// </returns>
	Task<IEnumerable<TimezoneRequest>> GetTimezonesAsync(CancellationToken cancellationToken = default);
}
