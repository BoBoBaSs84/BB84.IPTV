using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents a service that provides access to various repositories.
/// </summary>
internal sealed class RepositoryService : IRepositoryService
{
	private readonly IDatabaseContext _context;
	private readonly Lazy<ICategoryRepository> _categoryRepository;
	private readonly Lazy<IChannelRepository> _channelRepository;
	private readonly Lazy<ICountryRepository> _countryRepository;
	private readonly Lazy<IFeedRepository> _feedRepository;
	private readonly Lazy<IGuideRepository> _guideRepository;
	private readonly Lazy<ILanguageRepository> _languageRepository;
	private readonly Lazy<ILogoRepository> _logoRepository;
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
		_feedRepository = new Lazy<IFeedRepository>(() => new FeedRepository(_context));
		_guideRepository = new Lazy<IGuideRepository>(() => new GuideRepository(_context));
		_languageRepository = new Lazy<ILanguageRepository>(() => new LanguageRepository(_context));
		_logoRepository = new Lazy<ILogoRepository>(() => new LogoRepository(_context));
		_streamRepository = new Lazy<IStreamRepository>(() => new StreamRepository(_context));
	}

	public ICategoryRepository Categories => _categoryRepository.Value;
	public IChannelRepository Channels => _channelRepository.Value;
	public ICountryRepository Countries => _countryRepository.Value;
	public IFeedRepository Feeds => _feedRepository.Value;
	public IGuideRepository Guides => _guideRepository.Value;
	public ILanguageRepository Languages => _languageRepository.Value;
	public ILogoRepository Logos => _logoRepository.Value;
	public IStreamRepository Streams => _streamRepository.Value;


	public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
		=> await _context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);

	public async Task<int> CommitChangesAsync(CancellationToken token = default)
		=> await _context.SaveChangesAsync(token).ConfigureAwait(false);

	public async Task<bool> CreateDatabaseAsync(CancellationToken token = default)
	{
		bool databaseDeleted = await _context.Database
			.EnsureDeletedAsync(token)
			.ConfigureAwait(false);

		bool databaseCreated = await _context.Database
			.EnsureCreatedAsync(token)
			.ConfigureAwait(false);

		return databaseDeleted && databaseCreated;
	}
}
