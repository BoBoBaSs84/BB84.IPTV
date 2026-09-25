// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents a service that provides access to various repositories.
/// </summary>
internal sealed class RepositoryService : IRepositoryService
{
	private const string CountUserTablesSql =
		"SELECT COUNT(*) AS \"Value\" FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT IN ('__EFMigrationsHistory', '__EFMigrationsLock')";

	private readonly IDatabaseContext _context;
	private readonly Lazy<ICategoryRepository> _categoryRepository;
	private readonly Lazy<IChannelRepository> _channelRepository;
	private readonly Lazy<ICountryRepository> _countryRepository;
	private readonly Lazy<ICustomChannelRepository> _customChannelRepository;
	private readonly Lazy<IFeedRepository> _feedRepository;
	private readonly Lazy<IGuideRepository> _guideRepository;
	private readonly Lazy<IGuideMappingRepository> _guideMappingRepository;
	private readonly Lazy<ILanguageRepository> _languageRepository;
	private readonly Lazy<ILogoRepository> _logoRepository;
	private readonly Lazy<IPlaylistRepository> _playlistRepository;
	private readonly Lazy<IPlaylistEntryRepository> _playlistEntryRepository;
	private readonly Lazy<IStreamRepository> _streamRepository;

	/// <summary>
	/// Initializes a new instance of the <see cref="RepositoryService"/> class.
	/// </summary>
	/// <param name="context">The database context used for repository operations.</param>
	public RepositoryService(IDatabaseContext context)
	{
		_context = context;
		_categoryRepository = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
		_channelRepository = new Lazy<IChannelRepository>(() => new ChannelRepository(_context));
		_countryRepository = new Lazy<ICountryRepository>(() => new CountryRepository(_context));
		_customChannelRepository = new Lazy<ICustomChannelRepository>(() => new CustomChannelRepository(_context));
		_feedRepository = new Lazy<IFeedRepository>(() => new FeedRepository(_context));
		_guideRepository = new Lazy<IGuideRepository>(() => new GuideRepository(_context));
		_guideMappingRepository = new Lazy<IGuideMappingRepository>(() => new GuideMappingRepository(_context));
		_languageRepository = new Lazy<ILanguageRepository>(() => new LanguageRepository(_context));
		_logoRepository = new Lazy<ILogoRepository>(() => new LogoRepository(_context));
		_playlistRepository = new Lazy<IPlaylistRepository>(() => new PlaylistRepository(_context));
		_playlistEntryRepository = new Lazy<IPlaylistEntryRepository>(() => new PlaylistEntryRepository(_context));
		_streamRepository = new Lazy<IStreamRepository>(() => new StreamRepository(_context));
	}

	public ICategoryRepository Categories => _categoryRepository.Value;
	public IChannelRepository Channels => _channelRepository.Value;
	public ICountryRepository Countries => _countryRepository.Value;
	public ICustomChannelRepository CustomChannels => _customChannelRepository.Value;
	public IFeedRepository Feeds => _feedRepository.Value;
	public IGuideRepository Guides => _guideRepository.Value;
	public IGuideMappingRepository GuideMappings => _guideMappingRepository.Value;
	public ILanguageRepository Languages => _languageRepository.Value;
	public ILogoRepository Logos => _logoRepository.Value;
	public IPlaylistRepository Playlists => _playlistRepository.Value;
	public IPlaylistEntryRepository PlaylistEntries => _playlistEntryRepository.Value;
	public IStreamRepository Streams => _streamRepository.Value;


	public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
		=> await _context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);

	public async Task<int> CommitChangesAsync(CancellationToken token = default)
		=> await _context.SaveChangesAsync(token).ConfigureAwait(false);

	public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
	{
		if (await IsDatabaseWithoutMigrationsAsync(cancellationToken).ConfigureAwait(false))
		{
			await _context.Database
				.EnsureDeletedAsync(cancellationToken)
				.ConfigureAwait(false);
		}

		await _context.Database
			.MigrateAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<int> ResetCatalogAsync(CancellationToken cancellationToken = default)
	{
		int deleted = 0;
		deleted += await _context.Set<StreamEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<LogoEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<GuideEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<FeedEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<ChannelEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<LanguageEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<CountryEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		deleted += await _context.Set<CategoryEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		return deleted;
	}

	/// <summary>
	/// Detects a database created with <c>EnsureCreated</c> before migrations were introduced:
	/// it has tables, but no migration history.
	/// </summary>
	private async Task<bool> IsDatabaseWithoutMigrationsAsync(CancellationToken cancellationToken)
	{
		IEnumerable<string> appliedMigrations = await _context.Database
			.GetAppliedMigrationsAsync(cancellationToken)
			.ConfigureAwait(false);

		if (appliedMigrations.Any())
			return false;

		int tableCount = await _context.Database
			.SqlQueryRaw<int>(CountUserTablesSql)
			.SingleAsync(cancellationToken)
			.ConfigureAwait(false);

		return tableCount > 0;
	}
}