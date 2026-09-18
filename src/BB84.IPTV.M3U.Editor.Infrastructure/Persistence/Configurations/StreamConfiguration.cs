using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration for the <see cref="StreamEntity"/> entity.
/// </summary>
internal sealed class StreamConfiguration : ConfigurationBase<StreamEntity>
{
	public override void Configure(EntityTypeBuilder<StreamEntity> builder)
	{
		builder.ToTable("Streams");

		builder.HasIndex(i => i.Channel)
			.IsUnique(false);

		builder.HasIndex(i => i.Feed)
			.IsUnique(false);

		base.Configure(builder);
	}
}
