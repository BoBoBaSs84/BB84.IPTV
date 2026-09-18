// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Infrastructure.Common;

/// <summary>
/// Represents constant values used throughout the BB84.IPTV project.
/// </summary>
public static class Constants
{
	/// <summary>
	/// The name of the client used for IPTV data retrieval.
	/// </summary>
	public const string HttpClientName = "BB84.IPTV.HttpClient";

	/// <summary>
	/// The media type to be used in HTTP requests and responses.
	/// </summary>
	public const string HttpClientMediaType = "application/json";

	/// <summary>
	/// The base address for the HttpClient.
	/// </summary>
	public const string HttpClientBaseAddress = "https://iptv-org.github.io";

	/// <summary>
	/// The relative path to the channels JSON file.
	/// </summary>
	public const string ChannelsJsonPath = "api/channels.json";

	/// <summary>
	/// The relative path to the feeds JSON file.
	/// </summary>
	public const string FeedsJsonPath = "api/feeds.json";

	/// <summary>
	/// The relative path to the logos JSON file.
	/// </summary>
	public const string LogosJsonPath = "api/logos.json";

	/// <summary>
	/// The relative path to the streams JSON file.
	/// </summary>
	public const string StreamsJsonPath = "api/streams.json";

	/// <summary>
	/// The relative path to the guides JSON file.
	/// </summary>
	public const string GuidesJsonPath = "api/guides.json";

	/// <summary>
	/// The relative path to the categories JSON file.
	/// </summary>
	public const string CategoriesJsonPath = "api/categories.json";

	/// <summary>
	/// The relative path to the languages JSON file.
	/// </summary>
	public const string LanguagesJsonPath = "api/languages.json";

	/// <summary>
	/// The relative path to the countries JSON file.
	/// </summary>
	public const string CountriesJsonPath = "api/countries.json";

	/// <summary>
	/// The relative path to the subdivisions JSON file.
	/// </summary>
	public const string SubdivisionsJsonPath = "api/subdivisions.json";

	/// <summary>
	/// The relative path to the cities JSON file.
	/// </summary>
	public const string CitiesJsonPath = "api/cities.json";

	/// <summary>
	/// The relative path to the regions JSON file.
	/// </summary>
	public const string RegionsJsonPath = "api/regions.json";

	/// <summary>
	/// The relative path to the timezones JSON file.
	/// </summary>
	public const string TimezonesJsonPath = "api/timezones.json";

	/// <summary>
	/// The relative path to the blocklist JSON file.
	/// </summary>
	public const string BlocklistJsonPath = "api/blocklist.json";
}
