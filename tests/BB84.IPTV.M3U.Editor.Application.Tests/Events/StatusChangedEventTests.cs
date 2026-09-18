using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class StatusChangedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string text = "Status updated";
		bool autoClear = true;
		int duration = 5000;

		StatusChangedEvent statusChangedEvent = new(text, autoClear, duration);

		Assert.AreNotEqual(Guid.Empty, statusChangedEvent.Id);
		Assert.AreEqual(text, statusChangedEvent.Text);
		Assert.AreEqual(autoClear, statusChangedEvent.AutoClear);
		Assert.AreEqual(duration, statusChangedEvent.Duration);
	}
}
