using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class SettingsLoadedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		SettingsLoadedEvent settingsLoadedEvent = new();

		Assert.AreNotEqual(Guid.Empty, settingsLoadedEvent.Id);
	}
}
