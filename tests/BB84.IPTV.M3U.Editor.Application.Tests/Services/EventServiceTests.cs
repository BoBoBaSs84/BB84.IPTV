using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class EventServiceTests
{
	private readonly EventService _sut;

	public EventServiceTests()
		=> _sut = new(NullLogger<EventService>.Instance);

	[TestMethod]
	public void SubscribeShouldInvokeHandlerWhenEventIsPublished()
	{
		bool eventHandled = false;
		StatusChangedEvent @event = new("TestEvent");
		_sut.Subscribe<StatusChangedEvent>(e =>
		{
			eventHandled = true;
			Assert.AreEqual(@event.Id, e.Id);
			Assert.AreEqual(@event.OccurredAt, e.OccurredAt);
			Assert.AreEqual(@event.Text, e.Text);
		});

		_sut.Publish(@event);

		Assert.IsTrue(eventHandled, "The event handler was not invoked.");
	}
}
