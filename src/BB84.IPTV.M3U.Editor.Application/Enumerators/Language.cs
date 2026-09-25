// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents the supported languages for the application.
/// </summary>
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
