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
	/// Gets the projection of a <see cref="LanguageEntity"/> to the <see cref="CatalogFilterValue"/> a catalog search can be filtered by.
	/// </summary>
	internal static Expression<Func<LanguageEntity, CatalogFilterValue>> LanguageToFilterValue { get; }
		= language => new CatalogFilterValue(language.Code, language.Name);

	/// <summary>
	/// Converts a <see cref="LanguageRequest"/> to a <see cref="LanguageEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static LanguageEntity ToEntity(this LanguageRequest request) => new()
	{
		Code = request.Code,
		Name = request.Name
	};
}
