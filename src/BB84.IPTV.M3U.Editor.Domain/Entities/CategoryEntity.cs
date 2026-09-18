using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a category in the IPTV system.
/// </summary>
public sealed class CategoryEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the category identifier.
	/// </summary>
	public required string Category { get; set; }

	/// <summary>
	/// Gets or sets the name of the category.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets a short description of the category.
	/// </summary>
	public required string Description { get; set; }
}
