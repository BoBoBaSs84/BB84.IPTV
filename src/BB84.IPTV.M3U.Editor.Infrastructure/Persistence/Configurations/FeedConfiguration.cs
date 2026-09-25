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
/// Represents the configuration for the <see cref="FeedEntity"/> entity.
/// </summary>
internal sealed class FeedConfiguration : ConfigurationBase<FeedEntity>
{
	public override void Configure(EntityTypeBuilder<FeedEntity> builder)
	{
		builder.ToTable("Feeds");

		builder.HasIndex(i => i.Feed)
			.IsUnique(false);

		builder.HasIndex(i => i.Channel)
			.IsUnique(false);

		builder.Property(p => p.AltNames)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		builder.Property(p => p.BroadcastArea)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		builder.Property(p => p.Timezones)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		builder.Property(p => p.Languages)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		base.Configure(builder);
	}
}
