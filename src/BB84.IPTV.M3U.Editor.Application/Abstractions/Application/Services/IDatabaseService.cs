using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the database service interface, which is responsible for managing the data storage and retrieval.
/// </summary>
public interface IDatabaseService
{
	/// <summary>
	/// Checks the availability of the database, ensuring that it is accessible and operational for the application's needs.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing a boolean value that indicates
	/// the result of the database availability check,
	/// indicating whether the database is available and operational.
	/// </returns>
	Task<bool> CheckDatabaseAvailabilityAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Prepares the database for a catalog import: brings the schema up to date and deletes the previously
	/// imported catalog data. Playlists are kept.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing a boolean value indicating
	/// whether the database was successfully prepared.
	/// </returns>
	Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates the database or brings its schema up to date, called once at application start.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task MigrateDatabaseAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Imports all reference data into the database, including categories, countries, languages, and channels.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>
	/// A <see cref="DatabaseImportResponse"/> object containing the count of imported records for each repository.
	/// </returns>
	Task<DatabaseImportResponse> ImportDatabaseAsync(CancellationToken cancellationToken = default);
}
