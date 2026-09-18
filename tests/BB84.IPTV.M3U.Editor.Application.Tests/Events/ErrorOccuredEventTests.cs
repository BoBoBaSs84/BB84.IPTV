using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class ErrorOccuredEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string errorMessage = "An error occurred.";
		InvalidOperationException exception = new("Invalid operation.");

		ErrorOccuredEvent errorEvent = new(errorMessage, exception);

		Assert.AreNotEqual(Guid.Empty, errorEvent.Id);
		Assert.AreEqual(errorMessage, errorEvent.Message);
		Assert.AreEqual(exception, errorEvent.Exception);
	}
}
