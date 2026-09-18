using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class ExitRequestedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		ExitRequestedEvent exitRequestedEvent = new();

		Assert.AreNotEqual(Guid.Empty, exitRequestedEvent.Id);
	}
}
