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
/// Represents the configuration for the <see cref="PlaylistEntryEntity"/> entity.
/// </summary>
internal sealed class PlaylistEntryConfiguration : ConfigurationBase<PlaylistEntryEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<PlaylistEntryEntity> builder)
	{
		builder.ToTable("PlaylistEntries");

		builder.HasIndex(e => new { e.PlaylistId, e.Position })
			.IsUnique();

		builder.HasIndex(e => e.Channel)
			.IsUnique(false);

		base.Configure(builder);
	}
}