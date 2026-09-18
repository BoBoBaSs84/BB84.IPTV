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
