// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Services;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class ChannelsXmlSerializerTests
{
	private readonly ChannelsXmlSerializer _sut = new();

	[TestMethod]
	public void ShouldWriteTheFormatOfTheEpgGrabber()
	{
		string xml = _sut.Serialize([CreateMapping()]);

		Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml);
		Assert.Contains("<channels>", xml);
		Assert.Contains("<channel site=\"example.com\" site_id=\"123\" lang=\"de\" xmltv_id=\"DasErste.de\">Das Erste</channel>", xml);
		Assert.Contains("</channels>", xml);
	}

	[TestMethod]
	public void ShouldWriteAnEmptyRootWithoutMappings()
	{
		string xml = _sut.Serialize([]);

		Assert.Contains("<channels />", xml);
	}

	[TestMethod]
	public void ShouldLeaveOutAMappingThatCannotBeGrabbed()
	{
		string xml = _sut.Serialize(
		[
			CreateMapping(),
			CreateMapping(site: null, title: "No site"),
			CreateMapping(siteId: null, title: "No site id"),
			CreateMapping(xmltvId: null, title: "No xmltv id")
		]);

		Assert.Contains("Das Erste", xml);
		Assert.DoesNotContain("No site", xml);
		Assert.DoesNotContain("No site id", xml);
		Assert.DoesNotContain("No xmltv id", xml);
	}

	[TestMethod]
	public void ShouldEscapeWhatXmlCannotHold()
	{
		string xml = _sut.Serialize([CreateMapping(displayName: "Rock & Pop <HD>", siteId: "a\"b")]);

		Assert.Contains("Rock &amp; Pop &lt;HD&gt;", xml);
		Assert.Contains("site_id=\"a&quot;b\"", xml);
		Assert.DoesNotContain("<HD>", xml);
	}

	[TestMethod]
	public void ShouldWriteTheTitleWhenNoNameIsGiven()
	{
		string xml = _sut.Serialize([CreateMapping(displayName: null, title: "From the title")]);

		Assert.Contains(">From the title</channel>", xml);
	}

	[TestMethod]
	public void ShouldLeaveOutTheLanguageWhenItIsNotKnown()
	{
		string xml = _sut.Serialize([CreateMapping(lang: null)]);

		Assert.DoesNotContain("lang=", xml);
		Assert.Contains("site=\"example.com\"", xml);
	}

	[TestMethod]
	public void ShouldTrimTheValues()
	{
		string xml = _sut.Serialize([CreateMapping(site: "  example.com  ", siteId: " 123 ", xmltvId: " DasErste.de ")]);

		Assert.Contains("site=\"example.com\" site_id=\"123\"", xml);
		Assert.Contains("xmltv_id=\"DasErste.de\"", xml);
	}

	[TestMethod]
	public void ShouldThrowWithoutMappings()
		=> _ = Assert.ThrowsExactly<ArgumentNullException>(() => _sut.Serialize(null!));

	private static GuideMappingResponse CreateMapping(
		string? site = "example.com",
		string? siteId = "123",
		string? lang = "de",
		string? xmltvId = "DasErste.de",
		string? displayName = "Das Erste",
		string title = "Das Erste") => new()
		{
			EntryKey = xmltvId ?? title,
			Title = title,
			Site = site,
			SiteId = siteId,
			Lang = lang,
			XmltvId = xmltvId,
			DisplayName = displayName
		};
}