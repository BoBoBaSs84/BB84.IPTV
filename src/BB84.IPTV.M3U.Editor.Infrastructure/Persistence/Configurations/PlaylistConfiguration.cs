using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Represents the configuration for the <see cref="PlaylistEntity"/> entity.
/// </summary>
internal sealed class PlaylistConfiguration : ConfigurationBase<PlaylistEntity>
{
	/// <inheritdoc/>
	public override void Configure(EntityTypeBuilder<PlaylistEntity> builder)
	{
		builder.ToTable("Playlists");

		builder.HasIndex(p => p.Name)
			.IsUnique(false);

		builder.HasMany(p => p.Entries)
			.WithOne(e => e.Playlist)
			.HasForeignKey(e => e.PlaylistId)
			.OnDelete(DeleteBehavior.Cascade);

		base.Configure(builder);
	}
}