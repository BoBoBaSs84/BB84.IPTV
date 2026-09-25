// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

using Microsoft.EntityFrameworkCore;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence;

/// <summary>
/// Represents the application database context abstraction.
/// </summary>
/// <param name="options">The database context options.</param>
public sealed partial class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDatabaseContext
{
	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.AddInterceptors([]);
		base.OnConfiguring(optionsBuilder);
	}

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(IInfrastructureAssemblyMarker).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
