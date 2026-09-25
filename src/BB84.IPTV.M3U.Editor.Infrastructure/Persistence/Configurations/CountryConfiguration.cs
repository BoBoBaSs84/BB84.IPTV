// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Converters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration for the <see cref="CountryEntity"/> entity.
/// </summary>
internal sealed class CountryConfiguration : ConfigurationBase<CountryEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<CountryEntity> builder)
	{
		builder.ToTable("Countries");

		builder.Property(p => p.Code)
			.HasMaxLength(2)
			.IsRequired()
			.IsUnicode(false);

		builder.HasIndex(p => p.Code)
			.IsUnique(true);

		builder.Property(p => p.Name)
			.HasMaxLength(128);

		builder.Property(p => p.Flag)
			.HasMaxLength(1)
			.IsUnicode(true);

		builder.Property(p => p.Languages)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		base.Configure(builder);
	}
}
