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
	/// Gets the projection of a <see cref="LanguageEntity"/> to the <see cref="CatalogFilterValue"/> a catalog search can be filtered by.
	/// </summary>
	internal static Expression<Func<LanguageEntity, CatalogFilterValue>> LanguageToFilterValue { get; }
		= language => new CatalogFilterValue(language.Code, language.Name);

	/// <summary>
	/// Converts a <see cref="LanguageResponse"/> to a <see cref="LanguageEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static LanguageEntity ToEntity(this LanguageResponse response) => new()
	{
		Code = response.Code,
		Name = response.Name
	};

	/// <summary>
	/// Gets the key that identifies the language of the import.
	/// </summary>
	/// <param name="response">The imported language.</param>
	/// <returns>The key of the language.</returns>
	internal static string GetKey(this LanguageResponse response)
		=> CatalogKey.Of(response.Code);

	/// <summary>
	/// Gets the key that identifies the stored language.
	/// </summary>
	/// <param name="entity">The stored language.</param>
	/// <returns>The key of the language.</returns>
	internal static string GetKey(this LanguageEntity entity)
		=> CatalogKey.Of(entity.Code);

	/// <summary>
	/// Takes what the import holds into the stored language.
	/// </summary>
	/// <param name="entity">The stored language.</param>
	/// <param name="response">The imported language.</param>
	/// <returns><see langword="true"/> if the language changed.</returns>
	internal static bool Apply(this LanguageEntity entity, LanguageResponse response)
	{
		if (!Differs(entity.Name, response.Name))
			return false;

		entity.Name = response.Name;

		return true;
	}
}
