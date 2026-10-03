// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Settings;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the language the user interface is shown in, applied to the culture the localized
/// resources are read with.
/// </summary>
/// <remarks>
/// Only the user interface culture is set. How numbers and dates are written stays with the
/// culture of the operating system, because that is a regional setting and not a language.
/// </remarks>
internal sealed class LanguageService : ILanguageService
{
	private readonly GeneralSettings _settings;

	/// <summary>
	/// Initializes a new instance of the <see cref="LanguageService"/> class.
	/// </summary>
	/// <param name="applicationSettings">The current application settings.</param>
	public LanguageService(ApplicationSettings applicationSettings)
	{
		ArgumentNullException.ThrowIfNull(applicationSettings);

		_settings = applicationSettings.General;
	}

	/// <inheritdoc/>
	public Language? CurrentLanguage { get; private set; }

	/// <inheritdoc/>
	public void ApplyLanguage(Language language)
	{
		CultureInfo culture = GetCulture(language);

		CultureInfo.CurrentUICulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		CurrentLanguage = language;
	}

	/// <inheritdoc/>
	public bool HasPendingChanges()
		=> CurrentLanguage is { } currentLanguage && currentLanguage != _settings.Language;

	/// <summary>
	/// Reads the culture of <paramref name="language"/>, falling back to the default culture for a
	/// culture name the system does not know.
	/// </summary>
	/// <param name="language">The language to read the culture of.</param>
	/// <returns>The culture to read the localized resources with.</returns>
	private static CultureInfo GetCulture(Language language)
	{
		try
		{
			return CultureInfo.GetCultureInfo(language.GetCultureName());
		}
		catch (CultureNotFoundException)
		{
			// A language without a culture still has to leave the application usable.
			return CultureInfo.GetCultureInfo(LanguageExtensions.DefaultCultureName);
		}
	}
}
