using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories.Base;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository for managing <see cref="LanguageEntity"/> instances.
/// </summary>
/// <param name="dbContext">The database context to be used by the repository.</param>
internal sealed class LanguageRepository(IDbContext dbContext) : RepositoryBase<LanguageEntity>(dbContext), ILanguageRepository
{ }
