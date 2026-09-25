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
/// Represents the configuration for the <see cref="CategoryEntity"/> entity.
/// </summary>
internal sealed class CategoryConfiguration : ConfigurationBase<CategoryEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<CategoryEntity> builder)
	{
		builder.ToTable("Categories");

		builder.HasIndex(i => i.Category)
			.IsUnique(true);

		base.Configure(builder);
	}
}
