using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration for the <see cref="LanguageEntity"/> entity.
/// </summary>
internal sealed class LanguageConfiguration : ConfigurationBase<LanguageEntity>
{
	public override void Configure(EntityTypeBuilder<LanguageEntity> builder)
	{
		builder.ToTable("Languages");

		builder.Property(p => p.Code)
			.HasMaxLength(3)
			.IsUnicode(false);

		builder.HasIndex(i => i.Code)
			.IsUnique();

		base.Configure(builder);
	}
}
