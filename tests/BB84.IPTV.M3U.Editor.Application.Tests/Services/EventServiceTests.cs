using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Services;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class EventServiceTests
{
	private readonly Mock<ILoggerService<EventService>> _loggerServiceMock;
	private readonly EventService _sut;

	public EventServiceTests()
	{
		_loggerServiceMock = new();
		_sut = new(_loggerServiceMock.Object);
	}

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
