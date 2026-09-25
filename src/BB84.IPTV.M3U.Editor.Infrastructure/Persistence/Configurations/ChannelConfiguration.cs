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
/// Represents the configuration for the <see cref="ChannelEntity"/> entity.
/// </summary>
internal sealed class ChannelConfiguration : ConfigurationBase<ChannelEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<ChannelEntity> builder)
	{
		builder.ToTable("Channels");

		builder.HasIndex(p => p.Channel)
			.IsUnique();

		builder.Property(p => p.AltNames)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		builder.Property(p => p.Categories)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		builder.Property(p => p.Owners)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		base.Configure(builder);
	}
}
