using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class InformationOccuredEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string message = "This is a informational message.";

		InformationOccuredEvent informationEvent = new(message);

		Assert.AreNotEqual(Guid.Empty, informationEvent.Id);
		Assert.AreEqual(message, informationEvent.Message);
	}
}
