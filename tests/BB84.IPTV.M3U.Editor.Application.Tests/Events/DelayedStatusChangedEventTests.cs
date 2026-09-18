using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class DelayedStatusChangedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesWithDefaultDuration()
	{
		string text = "Delayed status";

		DelayedStatusChangedEvent delayedStatusChangedEvent = new(text);

		Assert.AreNotEqual(Guid.Empty, delayedStatusChangedEvent.Id);
		Assert.AreEqual(text, delayedStatusChangedEvent.Text);
		Assert.IsTrue(delayedStatusChangedEvent.AutoClear);
		Assert.AreEqual(2000, delayedStatusChangedEvent.Duration);
	}

	[TestMethod]
	public void ConstructorShouldInitializePropertiesWithSpecifiedDuration()
	{
		string text = "Delayed status";
		int duration = 4500;

		DelayedStatusChangedEvent delayedStatusChangedEvent = new(text, duration);

		Assert.AreNotEqual(Guid.Empty, delayedStatusChangedEvent.Id);
		Assert.AreEqual(text, delayedStatusChangedEvent.Text);
		Assert.IsTrue(delayedStatusChangedEvent.AutoClear);
		Assert.AreEqual(duration, delayedStatusChangedEvent.Duration);
	}
}
