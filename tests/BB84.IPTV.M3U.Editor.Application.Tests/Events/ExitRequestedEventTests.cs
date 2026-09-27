// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
