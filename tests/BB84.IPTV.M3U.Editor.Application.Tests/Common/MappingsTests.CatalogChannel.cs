// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

public sealed partial class MappingsTests
{
	[TestMethod]
	public void ToEntryShouldMapACatalogChannelWithItsFeed()
	{
		CatalogChannelResponse channel = new()
		{
			Channel = "DasErste.de",
			Feed = "SD",
			Name = "Das Erste",
			Country = "DE",
			Categories = ["news", "general"],
			StreamUrl = "https://example.com/ard.m3u8",
			LogoUrl = "https://logo.example/ard.png"
		};

		EntryModel entry = channel.ToEntry();

		Assert.AreEqual("Das Erste", entry.Title);
		Assert.AreEqual("https://example.com/ard.m3u8", entry.FilePath);
		Assert.AreEqual(-1, entry.Duration);
		Assert.AreEqual("DasErste.de", entry.Channel);
		Assert.AreEqual("SD", entry.Feed);
		Assert.AreEqual("DasErste.de@SD", entry.Metadata.TvgId);
		Assert.AreEqual("Das Erste", entry.Metadata.TvgName);
		Assert.AreEqual("https://logo.example/ard.png", entry.Metadata.TvgLogo);
		Assert.AreEqual("news", entry.Metadata.GroupTitle);
	}

	[TestMethod]
	public void ToEntryShouldUseTheChannelIdentifierAndCountryWithoutFeedAndCategories()
	{
		CatalogChannelResponse channel = new()
		{
			Channel = "Offline.de",
			Name = "Offline TV",
			Country = "DE",
			IsNsfw = true
		};

		EntryModel entry = channel.ToEntry();

		Assert.AreEqual("Offline.de", entry.Metadata.TvgId);
		Assert.AreEqual("DE", entry.Metadata.GroupTitle);
		Assert.AreEqual(string.Empty, entry.FilePath);
		Assert.IsTrue(entry.Metadata.Censored);
		Assert.IsNull(entry.Feed);
	}
}
