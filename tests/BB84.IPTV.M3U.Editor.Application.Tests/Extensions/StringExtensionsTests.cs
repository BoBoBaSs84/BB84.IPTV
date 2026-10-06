// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Extensions;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Extensions;

[TestClass]
public sealed class StringExtensionsTests
{
	[TestMethod]
	[DataRow("DasErste.de@SD", "DasErste.de", "SD", DisplayName = "channel and feed")]
	[DataRow("a@b@c", "a", "b@c", DisplayName = "everything after the first separator is the feed")]
	public void TryParseChannelFeedShouldSplitTheFullForm(string text, string channel, string feed)
	{
		bool parsed = text.TryParseChannelFeed(out string parsedChannel, out string parsedFeed);

		Assert.IsTrue(parsed);
		Assert.AreEqual(channel, parsedChannel);
		Assert.AreEqual(feed, parsedFeed);
	}

	[TestMethod]
	[DataRow(null, DisplayName = "no text")]
	[DataRow("", DisplayName = "an empty text")]
	[DataRow("DasErste.de", DisplayName = "no separator")]
	[DataRow("@SD", DisplayName = "no channel")]
	[DataRow("DasErste.de@", DisplayName = "no feed")]
	public void TryParseChannelFeedShouldRejectAnythingButTheFullForm(string? text)
	{
		bool parsed = text.TryParseChannelFeed(out string channel, out string feed);

		Assert.IsFalse(parsed);
		Assert.AreEqual(string.Empty, channel);
		Assert.AreEqual(string.Empty, feed);
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("   ", null)]
	[DataRow("  ARD ", "ARD")]
	public void TrimToNullShouldTurnABlankValueIntoNull(string? value, string? expected)
		=> Assert.AreEqual(expected, value.TrimToNull());
}
