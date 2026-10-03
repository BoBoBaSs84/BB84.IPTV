// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class CatalogSyncProgressEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		CatalogSyncProgressEvent syncEvent = new(CatalogKind.Channel, 12, 7, 2, 3, 5);

		Assert.AreNotEqual(Guid.Empty, syncEvent.Id);
		Assert.AreEqual(CatalogKind.Channel, syncEvent.Kind);
		Assert.AreEqual(12, syncEvent.Added);
		Assert.AreEqual(7, syncEvent.Updated);
		Assert.AreEqual(2, syncEvent.Removed);
		Assert.AreEqual(3, syncEvent.CompletedTasks);
		Assert.AreEqual(5, syncEvent.TotalTasks);
		Assert.AreEqual(60, syncEvent.ProgressPercentage, "Progress percentage should be (CompletedTasks/TotalTasks)*100");
	}
}
