// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
