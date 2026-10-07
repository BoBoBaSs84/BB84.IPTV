// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Gets the projection of a <see cref="CategoryEntity"/> to the <see cref="CatalogFilterValue"/> a catalog search can be filtered by.
	/// </summary>
	internal static Expression<Func<CategoryEntity, CatalogFilterValue>> CategoryToFilterValue { get; }
		= category => new CatalogFilterValue(category.Category, category.Name);

	/// <summary>
	/// Converts a <see cref="CategoryResponse"/> to a <see cref="CategoryEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static CategoryEntity ToEntity(this CategoryResponse response) => new()
	{
		Category = response.Id,
		Name = response.Name,
		Description = response.Description
	};

	/// <summary>
	/// Gets the key that identifies the category of the import.
	/// </summary>
	/// <param name="response">The imported category.</param>
	/// <returns>The key of the category.</returns>
	internal static string GetKey(this CategoryResponse response)
		=> CatalogKey.Of(response.Id);

	/// <summary>
	/// Gets the key that identifies the stored category.
	/// </summary>
	/// <param name="entity">The stored category.</param>
	/// <returns>The key of the category.</returns>
	internal static string GetKey(this CategoryEntity entity)
		=> CatalogKey.Of(entity.Category);

	/// <summary>
	/// Takes what the import holds into the stored category.
	/// </summary>
	/// <param name="entity">The stored category.</param>
	/// <param name="response">The imported category.</param>
	/// <returns><see langword="true"/> if the category changed.</returns>
	internal static bool Apply(this CategoryEntity entity, CategoryResponse response)
	{
		bool changed = false;

		if (Differs(entity.Name, response.Name))
		{
			entity.Name = response.Name;
			changed = true;
		}

		if (Differs(entity.Description, response.Description))
		{
			entity.Description = response.Description;
			changed = true;
		}

		return changed;
	}
}
