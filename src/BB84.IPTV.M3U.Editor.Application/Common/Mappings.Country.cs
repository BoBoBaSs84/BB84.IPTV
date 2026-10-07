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
	/// Gets the projection of a <see cref="CountryEntity"/> to the <see cref="CatalogFilterValue"/> a catalog search can be filtered by.
	/// </summary>
	internal static Expression<Func<CountryEntity, CatalogFilterValue>> CountryToFilterValue { get; }
		= country => new CatalogFilterValue(country.Code, country.Name);

	/// <summary>
	/// Converts a <see cref="CountryResponse"/> to a <see cref="CountryEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static CountryEntity ToEntity(this CountryResponse response) => new()
	{
		Code = response.Code,
		Name = response.Name,
		Languages = [.. response.Languages],
		Flag = response.Flag
	};

	/// <summary>
	/// Gets the key that identifies the country of the import.
	/// </summary>
	/// <param name="response">The imported country.</param>
	/// <returns>The key of the country.</returns>
	internal static string GetKey(this CountryResponse response)
		=> CatalogKey.Of(response.Code);

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
	/// <param name="response">The imported country.</param>
	/// <returns><see langword="true"/> if the country changed.</returns>
	internal static bool Apply(this CountryEntity entity, CountryResponse response)
	{
		bool changed = false;

		if (Differs(entity.Name, response.Name))
		{
			entity.Name = response.Name;
			changed = true;
		}

		if (Differs(entity.Flag, response.Flag))
		{
			entity.Flag = response.Flag;
			changed = true;
		}

		if (Differs(entity.Languages, response.Languages))
		{
			entity.Languages = [.. response.Languages];
			changed = true;
		}

		return changed;
	}
}
