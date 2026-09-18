using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class RestartRequestedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		RestartRequestedEvent restartRequestedEvent = new();

		Assert.AreNotEqual(Guid.Empty, restartRequestedEvent.Id);
	}
}
