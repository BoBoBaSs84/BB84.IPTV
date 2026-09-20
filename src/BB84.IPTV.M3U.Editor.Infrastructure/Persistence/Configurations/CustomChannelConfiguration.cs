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
/// Represents the configuration for the <see cref="CustomChannelEntity"/> entity.
/// </summary>
internal sealed class CustomChannelConfiguration : ConfigurationBase<CustomChannelEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<CustomChannelEntity> builder)
	{
		builder.ToTable("CustomChannels");

		builder.HasIndex(e => e.Name)
			.IsUnique(false);

		base.Configure(builder);
	}
}
