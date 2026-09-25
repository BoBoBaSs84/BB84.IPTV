// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Entities.Abstractions;
using BB84.EntityFrameworkCore.Repositories;
using BB84.EntityFrameworkCore.Repositories.Abstractions;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories.Base;

/// <summary>
/// Represents the base repository for entities with an integer identity.
/// </summary>
/// <typeparam name="TEntity">The type of the entity managed by the repository.</typeparam>
/// <param name="dbContext">The database context to be used by the repository.</param>
internal abstract class RepositoryBase<TEntity>(IDbContext dbContext) : IdentityRepository<TEntity, int>(dbContext)
		where TEntity : class, IIdentityEntity<int>
{ }
