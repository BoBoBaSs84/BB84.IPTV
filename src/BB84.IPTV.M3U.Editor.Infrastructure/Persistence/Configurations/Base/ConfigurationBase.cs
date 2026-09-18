using BB84.EntityFrameworkCore.Entities.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Configurations.Base;

/// <summary>
/// Represents the base configuration for identity entities with an integer primary key.
/// </summary>
/// <typeparam name="T">The type of the entity being configured.</typeparam>
internal abstract class ConfigurationBase<T> : IEntityTypeConfiguration<T>
	where T : class, IIdentityEntity<int>
{
	/// <inheritdoc/>
	public virtual void Configure(EntityTypeBuilder<T> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.HasColumnOrder(1)
			.ValueGeneratedOnAdd();

		builder.Ignore(e => e.Timestamp);
	}
}
