using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed class RepositoryServiceTests
{
	private const string CountHistory = "SELECT COUNT(*) FROM __EFMigrationsHistory";
	private const string CountPlaylistTables = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Playlists', 'PlaylistEntries')";

	[TestMethod]
	public async Task MigrateDatabaseAsyncShouldCreateTheSchema()
	{
		using SqliteTestDatabase database = SqliteTestDatabase.InFile();

		await database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		Assert.AreEqual(2, database.Scalar(CountPlaylistTables));
		Assert.IsGreaterThan(0L, database.Scalar(CountHistory));
	}

	[TestMethod]
	public async Task MigrateDatabaseAsyncShouldKeepDataOfAMigratedDatabase()
	{
		using SqliteTestDatabase database = SqliteTestDatabase.InFile();
		await database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);
		database.Execute("INSERT INTO Playlists (Name, Cache, Deinterlace, Refresh) VALUES ('Mine', 0, 0, 0)");

		await database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		Assert.AreEqual(1, database.Scalar("SELECT COUNT(*) FROM Playlists"));
	}

	[TestMethod]
	public async Task MigrateDatabaseAsyncShouldRecreateADatabaseWithoutMigrationHistory()
	{
		using SqliteTestDatabase database = SqliteTestDatabase.InFile();
		// A catalog-only database as created by EnsureCreated before migrations were introduced.
		database.Execute("CREATE TABLE Channels (Id INTEGER PRIMARY KEY, Channel TEXT NOT NULL)");
		database.Execute("INSERT INTO Channels (Channel) VALUES ('DasErste.de')");

		await database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		Assert.AreEqual(2, database.Scalar(CountPlaylistTables));
		Assert.AreEqual(0, database.Scalar("SELECT COUNT(*) FROM Channels"));
		Assert.IsGreaterThan(0L, database.Scalar(CountHistory));
	}
}