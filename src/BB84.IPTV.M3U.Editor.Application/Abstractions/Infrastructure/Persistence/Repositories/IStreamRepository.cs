using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository abstraction for managing <see cref="StreamEntity"/> instances.
/// </summary>
public interface IStreamRepository : IRepositoryBase<StreamEntity>
{ }
