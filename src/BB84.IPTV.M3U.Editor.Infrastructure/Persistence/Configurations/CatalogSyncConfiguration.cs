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
/// Represents the configuration for the <see cref="CatalogSyncEntity"/> entity.
/// </summary>
/// <remarks>
/// The kind is stored as its name, so the row keeps its meaning if the enum is ever reordered, and
/// the times are read back as UTC, because SQLite does not keep the kind of a date.
/// </remarks>
internal sealed class CatalogSyncConfiguration : ConfigurationBase<CatalogSyncEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<CatalogSyncEntity> builder)
	{
		builder.ToTable("CatalogSync");

		builder.HasIndex(i => i.Kind)
			.IsUnique(true);

		builder.Property(p => p.Kind)
			.HasConversion<string>()
			.HasMaxLength(16);

		builder.Property(p => p.FirstImported)
			.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

		builder.Property(p => p.LastChecked)
			.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

		builder.Property(p => p.LastChanged)
			.HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

		base.Configure(builder);
	}
}
