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
/// Represents the configuration for the <see cref="LogoEntity"/> entity.
/// </summary>
internal sealed class LogoConfiguration : ConfigurationBase<LogoEntity>
{
	public override void Configure(EntityTypeBuilder<LogoEntity> builder)
	{
		builder.ToTable("Logos");

		builder.HasIndex(i => i.Channel)
			.IsUnique(false);

		builder.HasIndex(i => i.Feed)
			.IsUnique(false);

		// The export looks a cached logo up by the URL an entry already carries.
		builder.HasIndex(i => i.Url)
			.IsUnique(false);

		builder.Property(p=>p.Tags)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		base.Configure(builder);
	}
}
