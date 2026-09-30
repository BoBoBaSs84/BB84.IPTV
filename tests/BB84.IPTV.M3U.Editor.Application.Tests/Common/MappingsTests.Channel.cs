// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

public sealed partial class MappingsTests
{
	[TestMethod]
	public void ChannelRequestToEntityShouldMapAllValues()
	{
		DateTime launched = new(1952, 12, 25, 0, 0, 0, DateTimeKind.Utc);
		ChannelRequest request = new("DasErste.de", "Das Erste", ["ARD"], "ARD", ["ARD", "WDR"], "DE", ["general", "news"], false, launched, null, null, "https://daserste.de");

		ChannelEntity entity = request.ToEntity();

		Assert.AreEqual("DasErste.de", entity.Channel);
		Assert.AreEqual("Das Erste", entity.Name);
		Assert.AreEqual("ARD", string.Join('|', entity.AltNames));
		Assert.AreEqual("ARD", entity.Network);
		Assert.AreEqual("ARD|WDR", string.Join('|', entity.Owners));
		Assert.AreEqual("DE", entity.Country);
		Assert.AreEqual("general|news", string.Join('|', entity.Categories));
		Assert.IsFalse(entity.IsNsfw);
		Assert.AreEqual(launched, entity.Launched);
		Assert.IsNull(entity.Closed);
		Assert.IsNull(entity.ReplacedBy);
		Assert.AreEqual("https://daserste.de", entity.Website);
	}

	[TestMethod]
	public void ChannelToResponseShouldTakeThePickedStreamAndLogo()
	{
		ChannelEntity channel = new() { Channel = "DasErste.de", Name = "Das Erste", Country = "DE", IsNsfw = true, Categories = ["news"] };
		StreamEntity stream = new() { Channel = "DasErste.de", Feed = "HD", Title = "Das Erste HD", Url = "https://example.com/ard.m3u8", Quality = "1080p" };
		LogoEntity logo = new() { Channel = "DasErste.de", Tags = [], Url = "https://logo.example/ard.png" };

		CatalogChannelResponse response = channel.ToResponse(stream, logo, ["deu", "eng"]);

		Assert.AreEqual("DasErste.de", response.Channel);
		Assert.AreEqual("HD", response.Feed);
		Assert.AreEqual("Das Erste", response.Name);
		Assert.AreEqual("DE", response.Country);
		Assert.AreEqual("deu|eng", string.Join('|', response.Languages));
		Assert.AreEqual("news", string.Join('|', response.Categories));
		Assert.IsTrue(response.IsNsfw);
		Assert.AreEqual("https://example.com/ard.m3u8", response.StreamUrl);
		Assert.AreEqual("1080p", response.Quality);
		Assert.AreEqual("https://logo.example/ard.png", response.LogoUrl);
	}

	[TestMethod]
	public void ChannelToResponseShouldHandleNoStreamNoLogoAndNullCategories()
	{
		// An empty collection is read from the database as null.
		ChannelEntity channel = new() { Channel = "Offline.de", Name = "Offline TV", Country = "DE", Categories = null! };

		CatalogChannelResponse response = channel.ToResponse(null, null, []);

		Assert.IsNull(response.Feed);
		Assert.IsNull(response.StreamUrl);
		Assert.IsNull(response.Quality);
		Assert.IsNull(response.LogoUrl);
		Assert.IsEmpty(response.Categories);
		Assert.IsEmpty(response.Languages);
	}

	[TestMethod]
	public void ChannelToInfoShouldProjectIdentifierNameAndCountry()
	{
		ChannelEntity channel = new() { Channel = "DasErste.de", Name = "Das Erste", Country = "DE" };

		ChannelInfo info = Mappings.ChannelToInfo.Compile()(channel);

		Assert.AreEqual(new ChannelInfo("DasErste.de", "Das Erste", "DE"), info);
	}

	[TestMethod]
	public void FilterValueSelectorsShouldProjectCodeAndName()
	{
		CatalogFilterValue country = Mappings.CountryToFilterValue.Compile()(new CountryEntity { Code = "DE", Name = "Germany" });
		CatalogFilterValue language = Mappings.LanguageToFilterValue.Compile()(new LanguageEntity { Code = "deu", Name = "German" });
		CatalogFilterValue category = Mappings.CategoryToFilterValue.Compile()(new CategoryEntity { Category = "news", Name = "News", Description = "News channels" });

		Assert.AreEqual(new CatalogFilterValue("DE", "Germany"), country);
		Assert.AreEqual(new CatalogFilterValue("deu", "German"), language);
		Assert.AreEqual(new CatalogFilterValue("news", "News"), category);
	}
}
