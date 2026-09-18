using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository abstraction for managing <see cref="ChannelEntity"/> instances.
/// </summary>
public interface IChannelRepository : IRepositoryBase<ChannelEntity>
{ }
