using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Installer;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Provides the application services on top of a private SQLite database for integration tests.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable
{
	private readonly SqliteConnection? _sharedConnection;
	private readonly string _connectionString;
	private readonly string? _filePath;

	private SqliteTestDatabase(string? filePath, Action<IServiceCollection>? configureServices = null)
	{
		_filePath = filePath;
		ServiceCollection services = new();
		services.RegisterApplicationServices();

		// Lets a test replace an infrastructure service, e.g. the downloader of the logo cache.
		configureServices?.Invoke(services);

		if (filePath is null)
		{
			// An in-memory database lives as long as its connection, so one connection is kept open and shared.
			_connectionString = "Data Source=:memory:";
			_sharedConnection = new SqliteConnection(_connectionString);
			_sharedConnection.Open();
			services.AddDbContext<IDatabaseContext, DatabaseContext>(options => options.UseSqlite(_sharedConnection));
		}
		else
		{
			// No pooling, so the file is released and can be deleted.
			_connectionString = $"Data Source={filePath};Pooling=False";
			services.AddDbContext<IDatabaseContext, DatabaseContext>(options => options.UseSqlite(_connectionString));
		}

		services.AddScoped<IRepositoryService, RepositoryService>();

		// The application services need a logger and, since the logo cache, a downloader and a
		// store; a test that cares about them replaces them through configureServices.
		services.AddLogging();
		services.TryAddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));
		services.TryAddSingleton<IDownloadService, OfflineDownloadService>();
		services.TryAddSingleton<ILogoStoreService, EmptyLogoStoreService>();

		Services = services.BuildServiceProvider();
	}

	/// <summary>
	/// Creates a test database in memory.
	/// </summary>
	/// <param name="configureServices">Adds or replaces services before the provider is built.</param>
	public static SqliteTestDatabase InMemory(Action<IServiceCollection>? configureServices = null)
		=> new(null, configureServices);

	/// <summary>
	/// Creates a test database in a new temporary file, deleted on dispose.
	/// </summary>
	public static SqliteTestDatabase InFile()
		=> new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db"));

	/// <summary>
	/// Gets the service provider with the application services and the test database.
	/// </summary>
	public ServiceProvider Services { get; }

	/// <summary>
	/// Runs <paramref name="action"/> with a repository service from a new scope.
	/// </summary>
	public async Task<T> WithRepositoryAsync<T>(Func<IRepositoryService, Task<T>> action)
	{
		using IServiceScope scope = Services.CreateScope();
		return await action(scope.ServiceProvider.GetRequiredService<IRepositoryService>()).ConfigureAwait(false);
	}

	/// <summary>
	/// Runs <paramref name="action"/> with a repository service from a new scope.
	/// </summary>
	public async Task WithRepositoryAsync(Func<IRepositoryService, Task> action)
	{
		using IServiceScope scope = Services.CreateScope();
		await action(scope.ServiceProvider.GetRequiredService<IRepositoryService>()).ConfigureAwait(false);
	}

	/// <summary>
	/// Answers no download, so a test never reaches the network.
	/// </summary>
	private sealed class OfflineDownloadService : IDownloadService
	{
		public Task<LogoDownloadResponse?> DownloadAsync(string url, string? eTag = null, CancellationToken cancellationToken = default)
			=> Task.FromResult<LogoDownloadResponse?>(null);
	}

	/// <summary>
	/// Holds nothing, so a test never writes into the application data directory.
	/// </summary>
	private sealed class EmptyLogoStoreService : ILogoStoreService
	{
		public Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default)
			=> throw new NotSupportedException("The test database does not store logos.");

		public bool Exists(string? path)
			=> false;

		public int Clear()
			=> 0;
	}

	/// <summary>
	/// Executes a raw SQL statement on the test database.
	/// </summary>
	public void Execute(string sql)
		=> Run(command => command.ExecuteNonQuery(), sql);

	/// <summary>
	/// Executes a raw SQL query returning a single number on the test database.
	/// </summary>
	public long Scalar(string sql)
		=> Run(command => (long)command.ExecuteScalar()!, sql);

	public void Dispose()
	{
		Services.Dispose();
		_sharedConnection?.Dispose();

		if (_filePath is not null && File.Exists(_filePath))
			File.Delete(_filePath);
	}

	private T Run<T>(Func<SqliteCommand, T> execute, string sql)
	{
		SqliteConnection connection = _sharedConnection ?? new SqliteConnection(_connectionString);
		try
		{
			if (connection.State is not System.Data.ConnectionState.Open)
				connection.Open();

			using SqliteCommand command = connection.CreateCommand();
			command.CommandText = sql;
			return execute(command);
		}
		finally
		{
			if (_sharedConnection is null)
				connection.Dispose();
		}
	}
}