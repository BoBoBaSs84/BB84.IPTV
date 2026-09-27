// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class StatusChangedEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string text = "Status updated";
		bool autoClear = true;
		int duration = 5000;

		StatusChangedEvent statusChangedEvent = new(text, autoClear, duration);

		Assert.AreNotEqual(Guid.Empty, statusChangedEvent.Id);
		Assert.AreEqual(text, statusChangedEvent.Text);
		Assert.AreEqual(autoClear, statusChangedEvent.AutoClear);
		Assert.AreEqual(duration, statusChangedEvent.Duration);
	}
}
