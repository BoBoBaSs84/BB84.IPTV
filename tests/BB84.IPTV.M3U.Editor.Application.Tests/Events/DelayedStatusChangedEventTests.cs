// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
