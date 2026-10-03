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
	/// Gets the projection of a <see cref="CountryEntity"/> to the <see cref="CatalogFilterValue"/> a catalog search can be filtered by.
	/// </summary>
	internal static Expression<Func<CountryEntity, CatalogFilterValue>> CountryToFilterValue { get; }
		= country => new CatalogFilterValue(country.Code, country.Name);

	/// <summary>
	/// Converts a <see cref="CountryRequest"/> to a <see cref="CountryEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static CountryEntity ToEntity(this CountryRequest request) => new()
	{
		Code = request.Code,
		Name = request.Name,
		Languages = [.. request.Languages],
		Flag = request.Flag
	};

	/// <summary>
	/// Gets the key that identifies the country of the import.
	/// </summary>
	/// <param name="request">The imported country.</param>
	/// <returns>The key of the country.</returns>
	internal static string GetKey(this CountryRequest request)
		=> CatalogKey.Of(request.Code);

	/// <summary>
	/// Gets the key that identifies the stored country.
	/// </summary>
	/// <param name="entity">The stored country.</param>
	/// <returns>The key of the country.</returns>
	internal static string GetKey(this CountryEntity entity)
		=> CatalogKey.Of(entity.Code);

	/// <summary>
	/// Takes what the import holds into the stored country.
	/// </summary>
	/// <param name="entity">The stored country.</param>
	/// <param name="request">The imported country.</param>
	/// <returns><see langword="true"/> if the country changed.</returns>
	internal static bool Apply(this CountryEntity entity, CountryRequest request)
	{
		bool changed = false;

		if (Differs(entity.Name, request.Name))
		{
			entity.Name = request.Name;
			changed = true;
		}

		if (Differs(entity.Flag, request.Flag))
		{
			entity.Flag = request.Flag;
			changed = true;
		}

		if (Differs(entity.Languages, request.Languages))
		{
			entity.Languages = [.. request.Languages];
			changed = true;
		}

		return changed;
	}
}
