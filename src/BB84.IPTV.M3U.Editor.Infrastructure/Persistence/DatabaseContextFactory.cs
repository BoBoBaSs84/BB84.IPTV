using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence;

/// <summary>
/// Creates the <see cref="DatabaseContext"/> for the EF Core tools, e.g. <c>dotnet ef migrations add</c>.
/// </summary>
/// <remarks>
/// Only used at design time, the application configures the context in <c>RegisterDatabaseContext</c>.
/// </remarks>
[ExcludeFromCodeCoverage(Justification = "Design-time only.")]
internal sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
	/// <inheritdoc/>
	public DatabaseContext CreateDbContext(string[] args)
	{
		DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
			.UseSqlite("Data Source=design-time.db")
			.Options;

		return new DatabaseContext(options);
	}
}