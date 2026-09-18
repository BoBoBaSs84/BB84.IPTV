using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents a service interface that provides access to various repositories.
/// </summary>
public interface IRepositoryService
{
	/// <summary>
	/// The repository instance for managing categories.
	/// </summary>
	ICategoryRepository Categories { get; }

	/// <summary>
	/// The repository instance for managing channels.
	/// </summary>
	IChannelRepository Channels { get; }

	/// <summary>
	/// The repository instance for managing countries.
	/// </summary>
	ICountryRepository Countries { get; }

	/// <summary>
	/// The repository instance for managing feeds.
	/// </summary>
	IFeedRepository Feeds { get; }

	/// <summary>
	/// The repository instance for managing guides.
	/// </summary>
	IGuideRepository Guides { get; }

	/// <summary>
	/// The repository instance for managing languages.
	/// </summary>
	ILanguageRepository Languages { get; }

	/// <summary>
	/// The repository instance for managing logos.
	/// </summary>
	ILogoRepository Logos { get; }

	/// <summary>
	/// The repository instance for managing streams.
	/// </summary>
	IStreamRepository Streams { get; }

	/// <summary>
	/// Indicates whether the service can establish a connection to the database, ensuring that it is accessible and
	/// operational for performing data operations.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can be used to cancel the operation if needed.</param>
	/// <returns>A boolean value indicating whether the service can successfully connect to the database.</returns>
	Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Should commit all the changes to the database context async.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can be used to cancel the operation if needed.</param>
	/// <returns>From the commit affected changes.</returns>
	Task<int> CommitChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates the database if it does not exist. If it already exists, this method will delete the
	/// existing database and create a new one.
	/// </summary>
	/// <remarks>
	/// This method should be called before accessing any repositories to ensure that the database
	/// is properly initialized.
	/// </remarks>
	/// <param name="cancellationToken">The cancellation token that can be used to cancel the operation if needed.</param>
	/// <returns>
	/// True if the database was created successfully; otherwise, false if the database already exists.
	/// </returns>
	Task<bool> CreateDatabaseAsync(CancellationToken cancellationToken = default);
}
