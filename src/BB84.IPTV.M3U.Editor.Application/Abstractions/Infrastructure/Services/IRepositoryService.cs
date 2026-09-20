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
	/// The repository instance for managing user defined channels.
	/// </summary>
	ICustomChannelRepository CustomChannels { get; }

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
	/// The repository instance for managing playlists.
	/// </summary>
	IPlaylistRepository Playlists { get; }

	/// <summary>
	/// The repository instance for managing playlist entries.
	/// </summary>
	IPlaylistEntryRepository PlaylistEntries { get; }

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
	/// Creates the database or brings its schema up to date by applying the pending migrations.
	/// </summary>
	/// <remarks>
	/// A database created before migrations were introduced only holds catalog data, which can be imported
	/// again, so it is deleted and created anew.
	/// </remarks>
	/// <param name="cancellationToken">The cancellation token that can be used to cancel the operation if needed.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	Task MigrateDatabaseAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes all catalog data imported from iptv-org (categories, countries, languages, channels, feeds,
	/// guides, logos and streams), so it can be imported again. Playlists are kept.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token that can be used to cancel the operation if needed.</param>
	/// <returns>The number of deleted rows.</returns>
	Task<int> ResetCatalogAsync(CancellationToken cancellationToken = default);
}
