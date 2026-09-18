using BB84.EntityFrameworkCore.Repositories.Abstractions;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;

/// <summary>
/// Represents the application database context interface.
/// </summary>
public interface IDatabaseContext : IDbContext
{ }
