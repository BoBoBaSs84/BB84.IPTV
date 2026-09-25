// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories.Base;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository for managing <see cref="PlaylistEntity"/> instances.
/// </summary>
/// <param name="dbContext">The database context to be used by the repository.</param>
internal sealed class PlaylistRepository(IDbContext dbContext) : RepositoryBase<PlaylistEntity>(dbContext), IPlaylistRepository
{ }