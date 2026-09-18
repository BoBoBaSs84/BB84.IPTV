using BB84.EntityFrameworkCore.Entities;

namespace BB84.IPTV.M3U.Editor.Domain.Entities.Base;

/// <summary>
/// Represents the base class for all entities in the application, providing a
/// common identifier property and equality comparison based on the identifier.
/// </summary>
public abstract class EntityBase : IdentityEntity<int>, IEquatable<EntityBase>
{
	/// <inheritdoc/>
	public bool Equals(EntityBase? other)
		=> other is not null && Id == other.Id;

	/// <inheritdoc/>
	public override bool Equals(object? obj)
		=> obj is EntityBase other && Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode()
		=> base.GetHashCode();
}
