// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration for the <see cref="GuideMappingEntity"/> entity.
/// </summary>
internal sealed class GuideMappingConfiguration : ConfigurationBase<GuideMappingEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<GuideMappingEntity> builder)
	{
		builder.ToTable("GuideMappings");

		// One mapping per entry of a playlist, and the mappings go when the playlist goes.
		builder.HasIndex(e => new { e.PlaylistId, e.EntryKey })
			.IsUnique();

		builder.HasOne(e => e.Playlist)
			.WithMany()
			.HasForeignKey(e => e.PlaylistId)
			.OnDelete(DeleteBehavior.Cascade);

		base.Configure(builder);
	}
}