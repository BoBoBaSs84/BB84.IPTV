// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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

	[TestMethod]
	public void UnsubscribeShouldStopInvokingTheHandler()
	{
		int handled = 0;
		Action<StatusChangedEvent> handler = _ => handled++;

		_sut.Subscribe(handler);
		_sut.Publish(new StatusChangedEvent("First"));

		_sut.Unsubscribe(handler);
		_sut.Publish(new StatusChangedEvent("Second"));

		Assert.AreEqual(1, handled, "The handler was invoked after it was unsubscribed.");
	}

	[TestMethod]
	public void UnsubscribeShouldKeepTheOtherHandlers()
	{
		int removed = 0;
		int kept = 0;
		Action<StatusChangedEvent> removedHandler = _ => removed++;

		_sut.Subscribe(removedHandler);
		_sut.Subscribe<StatusChangedEvent>(_ => kept++);

		_sut.Unsubscribe(removedHandler);
		_sut.Publish(new StatusChangedEvent("Test"));

		Assert.AreEqual(0, removed);
		Assert.AreEqual(1, kept);
	}

	[TestMethod]
	public void UnsubscribeShouldDoNothingWhenTheHandlerIsNotSubscribed()
	{
		int handled = 0;

		_sut.Unsubscribe<StatusChangedEvent>(_ => handled++);
		_sut.Publish(new StatusChangedEvent("Test"));

		Assert.AreEqual(0, handled);
	}

	[TestMethod]
	public void PublishShouldFinishWhenAHandlerUnsubscribesWhileItRuns()
	{
		int first = 0;
		int second = 0;
		Action<StatusChangedEvent>? unsubscribing = null;

		unsubscribing = _ =>
		{
			first++;
			_sut.Unsubscribe(unsubscribing!);
		};

		_sut.Subscribe(unsubscribing);
		_sut.Subscribe<StatusChangedEvent>(_ => second++);

		_sut.Publish(new StatusChangedEvent("First"));
		_sut.Publish(new StatusChangedEvent("Second"));

		Assert.AreEqual(1, first, "The handler that unsubscribed itself was invoked again.");
		Assert.AreEqual(2, second, "The handler after the one that unsubscribed was not invoked.");
	}

	[TestMethod]
	public void SubscribeShouldInvokeTheSameHandlerOncePerSubscription()
	{
		int handled = 0;
		Action<StatusChangedEvent> handler = _ => handled++;

		_sut.Subscribe(handler);
		_sut.Subscribe(handler);
		_sut.Publish(new StatusChangedEvent("Test"));

		Assert.AreEqual(2, handled);

		// One subscription is removed, the other one stays.
		_sut.Unsubscribe(handler);
		_sut.Publish(new StatusChangedEvent("Test"));

		Assert.AreEqual(3, handled);
	}

	[TestMethod]
	public void PublishShouldNotFailWithoutASubscriber()
	{
		_sut.Publish(new StatusChangedEvent("Test"));

		// Nothing to assert, publishing an event nobody listens to must not throw.
	}
}
