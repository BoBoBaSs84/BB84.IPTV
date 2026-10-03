// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

/// <summary>
/// Provides the name a list of the catalog is shown with.
/// </summary>
public static class CatalogKindExtensions
{
	/// <summary>
	/// Gets the localized name of the list.
	/// </summary>
	/// <param name="kind">The list of the catalog.</param>
	/// <returns>The name to show, the enumeration name if the list has none.</returns>
	public static string GetDisplayName(this CatalogKind kind) => kind switch
	{
		CatalogKind.Category => Resources.CatalogKindCategory,
		CatalogKind.Country => Resources.CatalogKindCountry,
		CatalogKind.Language => Resources.CatalogKindLanguage,
		CatalogKind.Channel => Resources.CatalogKindChannel,
		CatalogKind.Feed => Resources.CatalogKindFeed,
		CatalogKind.Guide => Resources.CatalogKindGuide,
		CatalogKind.Logo => Resources.CatalogKindLogo,
		CatalogKind.Stream => Resources.CatalogKindStream,
		_ => kind.ToString()
	};
}
