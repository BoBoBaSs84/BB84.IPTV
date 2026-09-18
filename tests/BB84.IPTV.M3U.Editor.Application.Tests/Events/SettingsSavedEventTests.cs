using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class SettingsSavedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		SettingsSavedEvent settingsSavedEvent = new();

		Assert.AreNotEqual(Guid.Empty, settingsSavedEvent.Id);
	}
}
