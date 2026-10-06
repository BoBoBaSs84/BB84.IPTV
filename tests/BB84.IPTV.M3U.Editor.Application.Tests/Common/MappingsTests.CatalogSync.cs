// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

public sealed partial class MappingsTests
{
	[TestMethod]
	public void TheKeyOfAnImportedRecordShouldMatchTheKeyOfTheStoredOne()
	{
		FeedRequest feed = new("DasErste.de", "SD", "Standard", [], true, [], [], [], "576i");
		GuideRequest guide = new("DasErste.de", null, "hoerzu.de", "ard", "ARD", "de");
		LogoRequest logo = new("DasErste.de", null, [], 256, 256, "png", "https://example.com/ard.png");
		StreamRequest stream = new("DasErste.de", "SD", "Das Erste", "https://example.com/ard.m3u8", null, null, null);

		Assert.AreEqual(feed.GetKey(), feed.ToEntity().GetKey());
		Assert.AreEqual(guide.GetKey(), guide.ToEntity().GetKey());
		Assert.AreEqual(logo.GetKey(), logo.ToEntity().GetKey());
		Assert.AreEqual(stream.GetKey(), stream.ToEntity().GetKey());

		// A feed that is not set must not make two different rows look the same.
		Assert.AreNotEqual(guide.GetKey(), (guide with { Feed = "SD" }).GetKey());
	}

	[TestMethod]
	public void ApplyingTheSameRecordShouldReportNoChange()
	{
		ChannelRequest channel = new("DasErste.de", "Das Erste", ["ARD"], "ARD", [], "DE", ["news"], false, null, null, null, null);
		FeedRequest feed = new("DasErste.de", "SD", "Standard", [], true, ["DE"], [], ["deu"], "576i");
		CategoryRequest category = new("news", "News", "What happens.");

		Assert.IsFalse(channel.ToEntity().Apply(channel));
		Assert.IsFalse(feed.ToEntity().Apply(feed));
		Assert.IsFalse(category.ToEntity().Apply(category));
	}

	[TestMethod]
	public void ApplyingAChangedRecordShouldTakeTheNewValues()
	{
		ChannelRequest stored = new("DasErste.de", "Das Erste", [], null, [], "DE", ["news"], false, null, null, null, null);
		ChannelEntity entity = stored.ToEntity();

		bool changed = entity.Apply(stored with { Name = "Das Erste HD", Categories = ["news", "general"], IsNsfw = true });

		Assert.IsTrue(changed);
		Assert.AreEqual("Das Erste HD", entity.Name);
		Assert.AreEqual("news|general", string.Join('|', entity.Categories));
		Assert.IsTrue(entity.IsNsfw);
	}

	[TestMethod]
	public void AnEmptyAndAMissingListShouldCountAsTheSame()
	{
		// The converter writes an empty list as null and reads null back as an empty list, so a
		// second run must not report every record with an empty list as updated.
		ChannelRequest request = new("ZDF.de", "ZDF", [], null, [], "DE", [], false, null, null, null, null);
		ChannelEntity entity = request.ToEntity();
		entity.AltNames = [];
		entity.Categories = [];
		entity.Owners = [];

		Assert.IsFalse(entity.Apply(request));
	}

	[TestMethod]
	public void ApplyingALogoShouldLeaveTheCacheAlone()
	{
		LogoRequest request = new("DasErste.de", null, ["horizontal"], 256, 256, "png", "https://example.com/ard.png");
		LogoEntity entity = request.ToEntity();
		entity.LocalPath = "C:\\cache\\ard.png";
		entity.ETag = "etag";
		entity.ContentHash = "hash";
		entity.FileSize = 1024;
		entity.DownloadedAt = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

		bool changed = entity.Apply(request with { Width = 512, Tags = [] });

		Assert.IsTrue(changed);
		Assert.AreEqual(512f, entity.Width);
		Assert.IsEmpty(entity.Tags);
		Assert.AreEqual("C:\\cache\\ard.png", entity.LocalPath);
		Assert.AreEqual("etag", entity.ETag);
		Assert.AreEqual("hash", entity.ContentHash);
		Assert.AreEqual(1024L, entity.FileSize);
		Assert.IsNotNull(entity.DownloadedAt);
	}

	[TestMethod]
	public void CatalogSyncToResponseShouldMapAllValues()
	{
		DateTime first = new(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc);
		CatalogSyncEntity entity = new()
		{
			Kind = CatalogKind.Channel,
			FirstImported = first,
			LastChecked = first.AddDays(10),
			LastChanged = first.AddDays(5),
			Added = 1,
			Updated = 2,
			Removed = 3
		};

		CatalogStatusResponse response = entity.ToResponse();

		Assert.AreEqual(CatalogKind.Channel, response.Kind);
		Assert.AreEqual(first, response.FirstImported);
		Assert.AreEqual(first.AddDays(10), response.LastChecked);
		Assert.AreEqual(first.AddDays(5), response.LastChanged);
		Assert.AreEqual(1, response.Added);
		Assert.AreEqual(2, response.Updated);
		Assert.AreEqual(3, response.Removed);
		Assert.IsTrue(response.IsImported);
	}

	[TestMethod]
	public void AListThatWasNeverReadShouldOnlyNameItsKind()
	{
		CatalogStatusResponse response = CatalogKind.Logo.ToEmptyStatus();

		Assert.AreEqual(CatalogKind.Logo, response.Kind);
		Assert.IsNull(response.FirstImported);
		Assert.IsNull(response.LastChecked);
		Assert.IsNull(response.LastChanged);
		Assert.AreEqual(0, response.Added);
		Assert.AreEqual(0, response.Updated);
		Assert.AreEqual(0, response.Removed);
		Assert.IsFalse(response.IsImported);
	}
}
