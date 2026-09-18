using BB84.EntityFrameworkCore.Entities.Abstractions;
using BB84.EntityFrameworkCore.Repositories.Abstractions;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Represents the base repository abstraction for managing entities with an integer identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity managed by the repository.</typeparam>
public interface IRepositoryBase<TEntity> : IIdentityRepository<TEntity, int>
		where TEntity : class, IIdentityEntity<int>
{ }
