// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents the supported languages for the application.
/// </summary>
[GenerateEnumeratorExtensions]
public enum Language
{
	/// <summary>
	/// Represents the English language.
	/// </summary>
	[Description("en-US")]
	English,
	/// <summary>
	/// Represents the Spanish language.
	/// </summary>
	[Description("es-ES")]
	Spanish,
	/// <summary>
	/// Represents the French language.
	/// </summary>
	[Description("fr-FR")]
	French,
	/// <summary>
	/// Represents the German language.
	/// </summary>
	[Description("de-DE")]
	German,
	/// <summary>
	/// Represents the Italian language.
	/// </summary>
	[Description("it-IT")]
	Italian
}

public static partial class LanguageExtensions
{
	/// <summary>
	/// The culture the application falls back to for a language without a culture name.
	/// </summary>
	public const string DefaultCultureName = "en-US";

	/// <summary>
	/// Gets the name of the culture the language stands for, taken from the
	/// <see cref="DescriptionAttribute"/> of the enumeration member.
	/// </summary>
	/// <param name="language">The language to read the culture name of.</param>
	/// <returns>The culture name, <see cref="DefaultCultureName"/> if the member names none.</returns>
	public static string GetCultureName(this Language language)
		=> language.IsDefinedFast() ? language.GetDescriptionFast() : DefaultCultureName;
}
