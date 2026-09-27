// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class LanguageChangedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string language = "German";

		LanguageChangedEvent languageChangedEvent = new(language);

		Assert.AreNotEqual(Guid.Empty, languageChangedEvent.Id);
		Assert.AreEqual(language, languageChangedEvent.Language);
	}
}
