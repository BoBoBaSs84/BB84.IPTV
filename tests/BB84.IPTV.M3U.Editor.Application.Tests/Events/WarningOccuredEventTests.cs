using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class WarningOccuredEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string message = "This is a warning message.";

		WarningOccuredEvent warningEvent = new(message);

		Assert.AreNotEqual(Guid.Empty, warningEvent.Id);
		Assert.AreEqual(message, warningEvent.Message);
	}
}
