// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
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
	/// Converts a <see cref="CategoryRequest"/> to a <see cref="CategoryEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static CategoryEntity ToEntity(this CategoryRequest request) => new()
	{
		Category = request.Id,
		Name = request.Name,
		Description = request.Description
	};
}
