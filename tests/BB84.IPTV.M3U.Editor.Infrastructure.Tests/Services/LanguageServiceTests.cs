// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

/// <summary>
/// The culture of the user interface belongs to the process, so these tests do not run next to
/// another one and put the culture they found back.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class LanguageServiceTests
{
	private CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
	private CultureInfo? _defaultUiCulture;

	[TestInitialize]
	public void Initialize()
	{
		_uiCulture = CultureInfo.CurrentUICulture;
		_defaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
	}

	[TestCleanup]
	public void Cleanup()
	{
		CultureInfo.CurrentUICulture = _uiCulture;
		CultureInfo.DefaultThreadCurrentUICulture = _defaultUiCulture;
	}

	[TestMethod]
	public void NoLanguageShouldBeAppliedBeforeTheApplicationStarts()
	{
		LanguageService sut = new(new ApplicationSettings());

		Assert.IsNull(sut.CurrentLanguage);
		Assert.IsFalse(sut.HasPendingChanges());
	}

	[TestMethod]
	public void ApplyingALanguageShouldSetTheCultureTheResourcesAreReadWith()
	{
		LanguageService sut = new(new ApplicationSettings());

		sut.ApplyLanguage(Language.German);

		Assert.AreEqual(Language.German, sut.CurrentLanguage);
		Assert.AreEqual("de-DE", CultureInfo.CurrentUICulture.Name);
		Assert.AreEqual("de-DE", CultureInfo.DefaultThreadCurrentUICulture?.Name);
	}

	[TestMethod]
	public void ApplyingALanguageShouldLeaveTheRegionalFormatsAlone()
	{
		CultureInfo culture = CultureInfo.CurrentCulture;
		LanguageService sut = new(new ApplicationSettings());

		sut.ApplyLanguage(Language.French);

		Assert.AreEqual(culture.Name, CultureInfo.CurrentCulture.Name);
	}

	[TestMethod]
	public void AChangedSettingShouldAskForARestart()
	{
		ApplicationSettings settings = new();
		LanguageService sut = new(settings);
		sut.ApplyLanguage(settings.General.Language);

		settings.General.Language = Language.Italian;

		Assert.IsTrue(sut.HasPendingChanges());
	}

	[TestMethod]
	public void TheAppliedSettingShouldNotAskForARestart()
	{
		ApplicationSettings settings = new();
		settings.General.Language = Language.Spanish;
		LanguageService sut = new(settings);

		sut.ApplyLanguage(settings.General.Language);

		Assert.IsFalse(sut.HasPendingChanges());
	}

	[TestMethod]
	public void TakingTheSettingBackShouldNotAskForARestart()
	{
		ApplicationSettings settings = new();
		LanguageService sut = new(settings);
		sut.ApplyLanguage(settings.General.Language);

		settings.General.Language = Language.German;
		settings.General.Language = Language.English;

		Assert.IsFalse(sut.HasPendingChanges());
	}

	[TestMethod]
	public void EveryLanguageShouldBeOneTheSystemCanApply()
	{
		LanguageService sut = new(new ApplicationSettings());

		foreach (Language language in Enum.GetValues<Language>())
		{
			sut.ApplyLanguage(language);

			Assert.AreEqual(language, sut.CurrentLanguage);
		}
	}

	[TestMethod]
	public void AnUnknownLanguageShouldFallBackToTheDefaultCulture()
	{
		LanguageService sut = new(new ApplicationSettings());

		sut.ApplyLanguage((Language)int.MaxValue);

		Assert.AreEqual("en-US", CultureInfo.CurrentUICulture.Name);
	}

	[TestMethod]
	public void TheSettingsAreRequired()
		=> _ = Assert.ThrowsExactly<ArgumentNullException>(() => new LanguageService(null!));
}
