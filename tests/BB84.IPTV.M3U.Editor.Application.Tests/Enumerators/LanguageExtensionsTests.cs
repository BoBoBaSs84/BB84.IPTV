// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Enumerators;

[TestClass]
public sealed class LanguageExtensionsTests
{
	[TestMethod]
	[DataRow(Language.English, "en-US")]
	[DataRow(Language.Spanish, "es-ES")]
	[DataRow(Language.French, "fr-FR")]
	[DataRow(Language.German, "de-DE")]
	[DataRow(Language.Italian, "it-IT")]
	public void EveryLanguageShouldNameItsCulture(Language language, string expected)
		=> Assert.AreEqual(expected, language.GetCultureName());

	[TestMethod]
	public void AnUnknownLanguageShouldFallBackToTheDefaultCulture()
		=> Assert.AreEqual(LanguageExtensions.DefaultCultureName, ((Language)int.MaxValue).GetCultureName());

	[TestMethod]
	public void EveryCultureNameShouldBeOneTheSystemKnows()
	{
		foreach (Language language in Enum.GetValues<Language>())
			_ = CultureInfo.GetCultureInfo(language.GetCultureName());
	}

	[TestMethod]
	public void EveryLanguageShouldNameAnOwnCulture()
	{
		string[] cultureNames = [.. Enum.GetValues<Language>().Select(language => language.GetCultureName())];

		Assert.HasCount(cultureNames.Length, cultureNames.Distinct(StringComparer.OrdinalIgnoreCase));
	}
}
