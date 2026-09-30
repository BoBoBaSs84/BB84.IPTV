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
	public void ToEntryShouldMapACustomChannel()
	{
		CustomChannelResponse channel = CreateCustomChannel();

		EntryModel entry = channel.ToEntry();

		Assert.AreEqual("Local camera", entry.Title);
		Assert.AreEqual("rtsp://192.168.12.1:554", entry.FilePath);
		Assert.AreEqual("camera", entry.Metadata.TvgId);
		Assert.AreEqual("Local camera", entry.Metadata.TvgName);
		Assert.AreEqual("/logos/camera.png", entry.Metadata.TvgLogo);
		Assert.AreEqual("Local", entry.Metadata.GroupTitle);
		Assert.IsNull(entry.Channel);
	}

	[TestMethod]
	public void CustomChannelToEntityShouldTrimNameAndUrl()
	{
		CustomChannelResponse channel = CreateCustomChannel(" Local camera ", " rtsp://192.168.12.1:554 ");

		CustomChannelEntity entity = channel.ToEntity();

		Assert.AreEqual("Local camera", entity.Name);
		Assert.AreEqual("rtsp://192.168.12.1:554", entity.Url);
		Assert.AreEqual("Local", entity.GroupTitle);
		Assert.AreEqual("camera", entity.TvgId);
		Assert.AreEqual("/logos/camera.png", entity.TvgLogo);
	}

	[TestMethod]
	public void CustomChannelApplyShouldOverwriteAllValues()
	{
		CustomChannelEntity entity = new() { Name = "Old", Url = "https://old.example", GroupTitle = "Old", TvgId = "old", TvgLogo = "old.png" };

		entity.Apply(CreateCustomChannel(" Local camera ", " rtsp://192.168.12.1:554 "));

		Assert.AreEqual("Local camera", entity.Name);
		Assert.AreEqual("rtsp://192.168.12.1:554", entity.Url);
		Assert.AreEqual("Local", entity.GroupTitle);
		Assert.AreEqual("camera", entity.TvgId);
		Assert.AreEqual("/logos/camera.png", entity.TvgLogo);
	}

	[TestMethod]
	public void CustomChannelToResponseShouldProjectAllValues()
	{
		CustomChannelEntity entity = new() { Id = 7, Name = "Local camera", Url = "rtsp://192.168.12.1:554", GroupTitle = "Local", TvgId = "camera", TvgLogo = "/logos/camera.png" };

		CustomChannelResponse response = Mappings.CustomChannelToResponse.Compile()(entity);

		Assert.AreEqual(7, response.Id);
		Assert.AreEqual("Local camera", response.Name);
		Assert.AreEqual("rtsp://192.168.12.1:554", response.Url);
		Assert.AreEqual("Local", response.GroupTitle);
		Assert.AreEqual("camera", response.TvgId);
		Assert.AreEqual("/logos/camera.png", response.TvgLogo);
	}

	private static CustomChannelResponse CreateCustomChannel(string name = "Local camera", string url = "rtsp://192.168.12.1:554") => new()
	{
		Id = 1,
		Name = name,
		Url = url,
		GroupTitle = "Local",
		TvgId = "camera",
		TvgLogo = "/logos/camera.png"
	};
}
