// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
