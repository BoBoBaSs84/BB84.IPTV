// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Contracts.Responses;

[TestClass]
public sealed class ResponseContractsTests
{
	[TestMethod]
	public void CategoryResponseShouldInitializePropertiesCorrectly()
	{
		CategoryResponse response = new("cat-1", "News", "News channels");

		Assert.AreEqual("cat-1", response.Id);
		Assert.AreEqual("News", response.Name);
		Assert.AreEqual("News channels", response.Description);
	}

	[TestMethod]
	public void ChannelResponseShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> altNames = ["Alt 1", "Alt 2"];
		IReadOnlyList<string> owners = ["Owner 1"];
		IReadOnlyList<string> categories = ["news", "sports"];
		DateTime launched = new(2020, 1, 1);
		DateTime closed = new(2024, 1, 1);

		ChannelResponse response = new("channel-1", "Channel", altNames, "Network", owners, "DE", categories, true, launched, closed, "channel-2", "https://example.com");

		Assert.AreEqual("channel-1", response.Id);
		Assert.AreEqual("Channel", response.Name);
		CollectionAssert.AreEqual(altNames.ToArray(), response.AltNames.ToArray());
		Assert.AreEqual("Network", response.Network);
		CollectionAssert.AreEqual(owners.ToArray(), response.Owners.ToArray());
		Assert.AreEqual("DE", response.Country);
		CollectionAssert.AreEqual(categories.ToArray(), response.Categories.ToArray());
		Assert.IsTrue(response.IsNsfw);
		Assert.AreEqual(launched, response.Launched);
		Assert.AreEqual(closed, response.Closed);
		Assert.AreEqual("channel-2", response.ReplacedBy);
		Assert.AreEqual("https://example.com", response.Website);
	}

	[TestMethod]
	public void CountryResponseShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> languages = ["deu", "eng"];
		CountryResponse response = new("Germany", "DE", languages, "🇩🇪");

		Assert.AreEqual("Germany", response.Name);
		Assert.AreEqual("DE", response.Code);
		CollectionAssert.AreEqual(languages.ToArray(), response.Languages.ToArray());
		Assert.AreEqual("🇩🇪", response.Flag);
	}

	[TestMethod]
	public void FeedResponseShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> altNames = ["Main Feed"];
		IReadOnlyList<string> broadcastArea = ["c/DE", "r/EU"];
		IReadOnlyList<string> timezones = ["Europe/Berlin"];
		IReadOnlyList<string> languages = ["de", "en"];

		FeedResponse response = new("channel-1", "feed-1", "Feed", altNames, true, broadcastArea, timezones, languages, "HD");

		Assert.AreEqual("channel-1", response.Channel);
		Assert.AreEqual("feed-1", response.Id);
		Assert.AreEqual("Feed", response.Name);
		CollectionAssert.AreEqual(altNames.ToArray(), response.AltNames.ToArray());
		Assert.IsTrue(response.IsMain);
		CollectionAssert.AreEqual(broadcastArea.ToArray(), response.BroadcastArea.ToArray());
		CollectionAssert.AreEqual(timezones.ToArray(), response.Timezones.ToArray());
		CollectionAssert.AreEqual(languages.ToArray(), response.Languages.ToArray());
		Assert.AreEqual("HD", response.Format);
	}

	[TestMethod]
	public void GuideResponseShouldInitializePropertiesCorrectly()
	{
		GuideResponse response = new("channel-1", "feed-1", "example.com", "site-id", "Site Name", "de");

		Assert.AreEqual("channel-1", response.Channel);
		Assert.AreEqual("feed-1", response.Feed);
		Assert.AreEqual("example.com", response.Site);
		Assert.AreEqual("site-id", response.SiteId);
		Assert.AreEqual("Site Name", response.SiteName);
		Assert.AreEqual("de", response.Lang);
	}

	[TestMethod]
	public void LanguageResponseShouldInitializePropertiesCorrectly()
	{
		LanguageResponse response = new("German", "deu");

		Assert.AreEqual("German", response.Name);
		Assert.AreEqual("deu", response.Code);
	}

	[TestMethod]
	public void LogoResponseShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> tags = ["dark", "square"];
		LogoResponse response = new("channel-1", "feed-1", tags, 100, 50, "PNG", "https://example.com/logo.png");

		Assert.AreEqual("channel-1", response.Channel);
		Assert.AreEqual("feed-1", response.Feed);
		CollectionAssert.AreEqual(tags.ToArray(), response.Tags.ToArray());
		Assert.AreEqual(100f, response.Width);
		Assert.AreEqual(50f, response.Height);
		Assert.AreEqual("PNG", response.Format);
		Assert.AreEqual("https://example.com/logo.png", response.Url);
	}

	[TestMethod]
	public void StreamResponseShouldInitializePropertiesCorrectly()
	{
		StreamResponse response = new("channel-1", "feed-1", "Stream Title", "https://example.com/stream.m3u8", "https://example.com", "agent", "1080p");

		Assert.AreEqual("channel-1", response.Channel);
		Assert.AreEqual("feed-1", response.Feed);
		Assert.AreEqual("Stream Title", response.Title);
		Assert.AreEqual("https://example.com/stream.m3u8", response.Url);
		Assert.AreEqual("https://example.com", response.Referrer);
		Assert.AreEqual("agent", response.UserAgent);
		Assert.AreEqual("1080p", response.Quality);
	}
}
