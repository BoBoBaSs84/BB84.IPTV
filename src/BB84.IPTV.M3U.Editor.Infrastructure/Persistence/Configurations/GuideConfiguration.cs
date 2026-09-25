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
/// Represents the configuration for the <see cref="GuideEntity"/> entity.
/// </summary>
internal sealed class GuideConfiguration : ConfigurationBase<GuideEntity>
{
	public override void Configure(EntityTypeBuilder<GuideEntity> builder)
	{
		builder.ToTable("Guides");

		builder.HasIndex(i => i.Channel)
			.IsUnique(false);

		builder.HasIndex(i => i.Feed)
			.IsUnique(false);

		builder.HasIndex(i => i.Site)
			.IsUnique(false);

		base.Configure(builder);
	}
}
