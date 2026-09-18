using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Contracts.Requests;

[TestClass]
public sealed class RequestContractsTests
{
	[TestMethod]
	public void BlocklistRequestShouldInitializePropertiesCorrectly()
	{
		BlocklistRequest request = new("channel-1", "dmca", "https://example.com/ref");

		Assert.AreEqual("channel-1", request.Channel);
		Assert.AreEqual("dmca", request.Reason);
		Assert.AreEqual("https://example.com/ref", request.Ref);
	}

	[TestMethod]
	public void CategoryRequestShouldInitializePropertiesCorrectly()
	{
		CategoryRequest request = new("cat-1", "News", "News channels");

		Assert.AreEqual("cat-1", request.Id);
		Assert.AreEqual("News", request.Name);
		Assert.AreEqual("News channels", request.Description);
	}

	[TestMethod]
	public void ChannelRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> altNames = ["Alt 1", "Alt 2"];
		IReadOnlyList<string> owners = ["Owner 1"];
		IReadOnlyList<string> categories = ["news", "sports"];
		DateTime launched = new(2020, 1, 1);
		DateTime closed = new(2024, 1, 1);

		ChannelRequest request = new("channel-1", "Channel", altNames, "Network", owners, "DE", categories, true, launched, closed, "channel-2", "https://example.com");

		Assert.AreEqual("channel-1", request.Id);
		Assert.AreEqual("Channel", request.Name);
		CollectionAssert.AreEqual(altNames.ToArray(), request.AltNames.ToArray());
		Assert.AreEqual("Network", request.Network);
		CollectionAssert.AreEqual(owners.ToArray(), request.Owners.ToArray());
		Assert.AreEqual("DE", request.Country);
		CollectionAssert.AreEqual(categories.ToArray(), request.Categories.ToArray());
		Assert.IsTrue(request.IsNsfw);
		Assert.AreEqual(launched, request.Launched);
		Assert.AreEqual(closed, request.Closed);
		Assert.AreEqual("channel-2", request.ReplacedBy);
		Assert.AreEqual("https://example.com", request.Website);
	}

	[TestMethod]
	public void CityRequestShouldInitializePropertiesCorrectly()
	{
		CityRequest request = new("DE", "DE-BY", "Munich", "MUC", "Q1726");

		Assert.AreEqual("DE", request.Country);
		Assert.AreEqual("DE-BY", request.Subdivision);
		Assert.AreEqual("Munich", request.Name);
		Assert.AreEqual("MUC", request.Code);
		Assert.AreEqual("Q1726", request.WikidataId);
	}

	[TestMethod]
	public void CountryRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> languages = ["deu", "eng"];
		CountryRequest request = new("Germany", "DE", languages, "🇩🇪");

		Assert.AreEqual("Germany", request.Name);
		Assert.AreEqual("DE", request.Code);
		CollectionAssert.AreEqual(languages.ToArray(), request.Languages.ToArray());
		Assert.AreEqual("🇩🇪", request.Flag);
	}

	[TestMethod]
	public void FeedRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> altNames = ["Main Feed"];
		IReadOnlyList<string> broadcastArea = ["c/DE", "r/EU"];
		IReadOnlyList<string> timezones = ["Europe/Berlin"];
		IReadOnlyList<string> languages = ["de", "en"];

		FeedRequest request = new("channel-1", "feed-1", "Feed", altNames, true, broadcastArea, timezones, languages, "HD");

		Assert.AreEqual("channel-1", request.Channel);
		Assert.AreEqual("feed-1", request.Id);
		Assert.AreEqual("Feed", request.Name);
		CollectionAssert.AreEqual(altNames.ToArray(), request.AltNames.ToArray());
		Assert.IsTrue(request.IsMain);
		CollectionAssert.AreEqual(broadcastArea.ToArray(), request.BroadcastArea.ToArray());
		CollectionAssert.AreEqual(timezones.ToArray(), request.Timezones.ToArray());
		CollectionAssert.AreEqual(languages.ToArray(), request.Languages.ToArray());
		Assert.AreEqual("HD", request.Format);
	}

	[TestMethod]
	public void GuideRequestShouldInitializePropertiesCorrectly()
	{
		GuideRequest request = new("channel-1", "feed-1", "example.com", "site-id", "Site Name", "de");

		Assert.AreEqual("channel-1", request.Channel);
		Assert.AreEqual("feed-1", request.Feed);
		Assert.AreEqual("example.com", request.Site);
		Assert.AreEqual("site-id", request.SiteId);
		Assert.AreEqual("Site Name", request.SiteName);
		Assert.AreEqual("de", request.Lang);
	}

	[TestMethod]
	public void LanguageRequestShouldInitializePropertiesCorrectly()
	{
		LanguageRequest request = new("German", "deu");

		Assert.AreEqual("German", request.Name);
		Assert.AreEqual("deu", request.Code);
	}

	[TestMethod]
	public void LogoRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> tags = ["dark", "square"];
		LogoRequest request = new("channel-1", "feed-1", tags, 100, 50, "PNG", "https://example.com/logo.png");

		Assert.AreEqual("channel-1", request.Channel);
		Assert.AreEqual("feed-1", request.Feed);
		CollectionAssert.AreEqual(tags.ToArray(), request.Tags.ToArray());
		Assert.AreEqual(100f, request.Width);
		Assert.AreEqual(50f, request.Height);
		Assert.AreEqual("PNG", request.Format);
		Assert.AreEqual("https://example.com/logo.png", request.Url);
	}

	[TestMethod]
	public void RegionRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> countries = ["DE", "FR"];
		RegionRequest request = new("EU", "Europe", countries);

		Assert.AreEqual("EU", request.Code);
		Assert.AreEqual("Europe", request.Name);
		CollectionAssert.AreEqual(countries.ToArray(), request.Countries.ToArray());
	}

	[TestMethod]
	public void StreamRequestShouldInitializePropertiesCorrectly()
	{
		StreamRequest request = new("channel-1", "feed-1", "Stream Title", "https://example.com/stream.m3u8", "https://example.com", "agent", "1080p");

		Assert.AreEqual("channel-1", request.Channel);
		Assert.AreEqual("feed-1", request.Feed);
		Assert.AreEqual("Stream Title", request.Title);
		Assert.AreEqual("https://example.com/stream.m3u8", request.Url);
		Assert.AreEqual("https://example.com", request.Referrer);
		Assert.AreEqual("agent", request.UserAgent);
		Assert.AreEqual("1080p", request.Quality);
	}

	[TestMethod]
	public void SubdivisionRequestShouldInitializePropertiesCorrectly()
	{
		SubdivisionRequest request = new("DE", "Bavaria", "DE-BY", "DE");

		Assert.AreEqual("DE", request.Country);
		Assert.AreEqual("Bavaria", request.Name);
		Assert.AreEqual("DE-BY", request.Code);
		Assert.AreEqual("DE", request.Parent);
	}

	[TestMethod]
	public void TimezoneRequestShouldInitializePropertiesCorrectly()
	{
		IReadOnlyList<string> countries = ["DE", "AT"];
		TimezoneRequest request = new("Europe/Berlin", "+01:00", countries);

		Assert.AreEqual("Europe/Berlin", request.Id);
		Assert.AreEqual("+01:00", request.UtcOffset);
		CollectionAssert.AreEqual(countries.ToArray(), request.Countries.ToArray());
	}
}
