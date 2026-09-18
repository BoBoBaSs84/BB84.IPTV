using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class ProgressChangedEventTests
{
	[TestMethod]
	public void ConstructorWithSingleValueShouldInitializePropertiesCorrectly()
	{
		int value = 42;

		ProgressChangedEvent progressChangedEvent = new(value);

		Assert.AreNotEqual(Guid.Empty, progressChangedEvent.Id);
		Assert.AreEqual(value, progressChangedEvent.Value);
		Assert.AreEqual(0, progressChangedEvent.Minimum);
		Assert.AreEqual(100, progressChangedEvent.Maximum);
	}

	[TestMethod]
	public void ConstructorWithMinAndMaxShouldInitializePropertiesCorrectly()
	{
		int value = 50;
		int minimum = 10;
		int maximum = 90;

		ProgressChangedEvent progressChangedEvent = new(value, minimum, maximum);

		Assert.AreNotEqual(Guid.Empty, progressChangedEvent.Id);
		Assert.AreEqual(value, progressChangedEvent.Value);
		Assert.AreEqual(minimum, progressChangedEvent.Minimum);
		Assert.AreEqual(maximum, progressChangedEvent.Maximum);
	}
}
