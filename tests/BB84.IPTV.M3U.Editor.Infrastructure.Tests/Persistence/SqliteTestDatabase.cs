using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Installer;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Provides the application services on top of a private SQLite database for integration tests.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable
{
	private readonly SqliteConnection? _sharedConnection;
	private readonly string _connectionString;
	private readonly string? _filePath;

	private SqliteTestDatabase(string? filePath)
	{
		_filePath = filePath;
		ServiceCollection services = new();
		services.RegisterApplicationServices();

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
		Services = services.BuildServiceProvider();
	}

	/// <summary>
	/// Creates a test database in memory.
	/// </summary>
	public static SqliteTestDatabase InMemory()
		=> new(null);

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