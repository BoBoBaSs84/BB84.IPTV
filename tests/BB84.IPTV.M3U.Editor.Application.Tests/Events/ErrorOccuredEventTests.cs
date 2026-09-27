// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
