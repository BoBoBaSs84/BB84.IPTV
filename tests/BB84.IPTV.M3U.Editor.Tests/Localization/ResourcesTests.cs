// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections;
using System.Globalization;
using System.Resources;

using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Properties;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Tests.Localization;

/// <summary>
/// Guards the localized resources of the views: a string the English resources name has to be in
/// every culture, and an enumeration a view shows needs a name for every member.
/// </summary>
[TestClass]
public sealed class ResourcesTests
{
	/// <summary>
	/// The cultures the application ships next to the English resources.
	/// </summary>
	private static readonly string[] TranslatedCultures = ["de-DE", "es-ES", "fr-FR", "it-IT"];

	/// <summary>
	/// The enumerations a combo box of a view shows, which the
	/// <see cref="Converters.EnumDisplayNameConverter"/> reads the names of.
	/// </summary>
	private static readonly Type[] ShownEnumerations =
	[
		typeof(Language),
		typeof(LogLevel),
		typeof(LogoPathStyle),
		typeof(MergeDuplicateMode),
		typeof(MergeDuplicateResolution),
		typeof(Deinterlace)
	];

	[TestMethod]
	[DataRow("de-DE")]
	[DataRow("es-ES")]
	[DataRow("fr-FR")]
	[DataRow("it-IT")]
	public void EveryCultureShouldHoldEveryString(string cultureName)
	{
		string[] missing = [.. NamesOf(CultureInfo.InvariantCulture)
			.Except(NamesOf(CultureInfo.GetCultureInfo(cultureName)), StringComparer.Ordinal)
			.Order(StringComparer.Ordinal)];

		Assert.IsEmpty(missing, $"{cultureName} is missing: {string.Join(", ", missing)}");
	}

	[TestMethod]
	public void NoCultureShouldHoldAStringTheEnglishResourcesDoNotName()
	{
		string[] english = [.. NamesOf(CultureInfo.InvariantCulture)];
		string[] extra = [.. TranslatedCultures
			.SelectMany(cultureName => NamesOf(CultureInfo.GetCultureInfo(cultureName))
				.Except(english, StringComparer.Ordinal)
				.Select(name => $"{cultureName}:{name}"))
			.Order(StringComparer.Ordinal)];

		Assert.IsEmpty(extra, string.Join(", ", extra));
	}

	[TestMethod]
	public void EveryShownEnumerationMemberShouldHaveAName()
	{
		string[] missing = [.. ShownEnumerations
			.SelectMany(type => Enum.GetNames(type).Select(name => $"Enum.{type.Name}.{name}"))
			.Where(key => Resources.ResourceManager.GetString(key, CultureInfo.InvariantCulture) is null)
			.Order(StringComparer.Ordinal)];

		Assert.IsEmpty(missing, string.Join(", ", missing));
	}

	/// <summary>
	/// Reads the names of the resources of a single culture, without falling back to its parents.
	/// </summary>
	/// <param name="culture">The culture to read, the invariant culture reads the English resources.</param>
	/// <returns>The names the culture holds.</returns>
	private static IEnumerable<string> NamesOf(CultureInfo culture)
	{
		ResourceSet? resourceSet = Resources.ResourceManager
			.GetResourceSet(culture, createIfNotExists: true, tryParents: false);

		Assert.IsNotNull(resourceSet, $"no resources for {culture.Name}");

		return [.. resourceSet.Cast<DictionaryEntry>().Select(entry => (string)entry.Key)];
	}
}
