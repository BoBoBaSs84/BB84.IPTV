// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents the response from a database import operation, containing the count of imported records for each repository.
/// </summary>
public sealed class DatabaseImportResponse
{
	/// <summary>
	/// Gets or initializes the number of categories that were imported.
	/// </summary>
	public int CategoriesImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of countries that were imported.
	/// </summary>
	public int CountriesImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of languages that were imported.
	/// </summary>
	public int LanguagesImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of channels that were imported.
	/// </summary>
	public int ChannelsImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of feeds that were imported.
	/// </summary>
	public int FeedsImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of guides that were imported.
	/// </summary>
	public int GuidesImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of logos that were imported.
	/// </summary>
	public int LogosImported { get; init; }

	/// <summary>
	/// Gets or initializes the number of streams that were imported.
	/// </summary>
	public int StreamsImported { get; init; }

	/// <summary>
	/// Gets the total number of records imported across all repositories.
	/// </summary>
	public int TotalImported => CategoriesImported + CountriesImported + LanguagesImported + ChannelsImported + FeedsImported + GuidesImported + LogosImported + StreamsImported;

	/// <summary>
	/// Gets a value indicating whether the import operation was successful (at least one record was imported).
	/// </summary>
	public bool IsSuccess => TotalImported > 0;
}
