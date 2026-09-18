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

		builder.Property(p=>p.Tags)
			.HasConversion<StringCollectionConverter>()
			.IsRequired(false);

		base.Configure(builder);
	}
}
