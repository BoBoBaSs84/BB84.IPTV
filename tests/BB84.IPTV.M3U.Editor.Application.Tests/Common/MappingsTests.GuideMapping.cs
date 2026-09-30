// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

public sealed partial class MappingsTests
{
	[TestMethod]
	public void GuideMappingToEntityShouldTrimValuesAndStoreBlanksAsNull()
	{
		GuideMappingResponse mapping = new()
		{
			EntryKey = "DasErste.de",
			Title = "Das Erste",
			Site = " example.com ",
			SiteId = "ard",
			Lang = "  ",
			XmltvId = " DasErste.de ",
			DisplayName = null
		};

		GuideMappingEntity entity = mapping.ToEntity(3);

		Assert.AreEqual(3, entity.PlaylistId);
		Assert.AreEqual("DasErste.de", entity.EntryKey);
		Assert.AreEqual("example.com", entity.Site);
		Assert.AreEqual("ard", entity.SiteId);
		Assert.IsNull(entity.Lang);
		Assert.AreEqual("DasErste.de", entity.XmltvId);
		Assert.IsNull(entity.DisplayName);
	}

	[TestMethod]
	public void GuideMappingToResponseShouldFallBackToTheTitleAsDisplayName()
	{
		GuideMappingEntity entity = new() { EntryKey = "key", Site = "example.com", SiteId = "ard", Lang = "de", XmltvId = "DasErste.de" };
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8") { Feed = "HD" };

		GuideMappingResponse response = entity.ToResponse(entry, "key", "DasErste.de");

		Assert.AreEqual("key", response.EntryKey);
		Assert.AreEqual("Das Erste", response.Title);
		Assert.AreEqual("example.com", response.Site);
		Assert.AreEqual("ard", response.SiteId);
		Assert.AreEqual("de", response.Lang);
		Assert.AreEqual("DasErste.de", response.XmltvId);
		Assert.AreEqual("Das Erste", response.DisplayName);
		Assert.AreEqual("DasErste.de", response.Channel);
		Assert.AreEqual("HD", response.Feed);
		Assert.IsTrue(response.IsStored);
	}

	[TestMethod]
	public void ToGuideMappingShouldPrefillFromTheGuide()
	{
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8", metadata: new MetadataModel { TvgId = "DasErste.de@HD" }) { Feed = "HD" };
		GuideEntity guide = new() { Channel = "DasErste.de", Site = "example.com", SiteId = "ard", SiteName = "Das Erste", Lang = "de" };

		GuideMappingResponse response = entry.ToGuideMapping("DasErste.de@HD", guide, "DasErste.de");

		Assert.AreEqual("DasErste.de@HD", response.EntryKey);
		Assert.AreEqual("example.com", response.Site);
		Assert.AreEqual("ard", response.SiteId);
		Assert.AreEqual("de", response.Lang);
		Assert.AreEqual("DasErste.de@HD", response.XmltvId);
		Assert.AreEqual("Das Erste", response.DisplayName);
		Assert.AreEqual("DasErste.de", response.Channel);
		Assert.IsFalse(response.IsStored);
	}

	[TestMethod]
	public void ToGuideMappingShouldLeaveTheSiteEmptyWithoutGuide()
	{
		EntryModel entry = new("Unknown", "https://example.com/unknown.m3u8");

		GuideMappingResponse response = entry.ToGuideMapping("https://example.com/unknown.m3u8", null, null);

		Assert.IsNull(response.Site);
		Assert.IsNull(response.SiteId);
		Assert.IsNull(response.Lang);
		Assert.IsNull(response.Channel);
		Assert.IsFalse(response.IsStored);
	}
}
