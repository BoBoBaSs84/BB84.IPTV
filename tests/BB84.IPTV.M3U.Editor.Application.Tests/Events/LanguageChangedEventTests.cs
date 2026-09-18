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
