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
	/// Creates the database, initializing all necessary tables and schema for the application.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>
	/// A task that represents the asynchronous operation, containing a boolean value indicating
	/// whether the database was successfully created.
	/// </returns>
	Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Imports all reference data into the database, including categories, countries, languages, and channels.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>
	/// A <see cref="DatabaseImportResponse"/> object containing the count of imported records for each repository.
	/// </returns>
	Task<DatabaseImportResponse> ImportDatabaseAsync(CancellationToken cancellationToken = default);
}
