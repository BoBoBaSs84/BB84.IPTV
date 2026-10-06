// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

[TestClass]
public sealed class CatalogLookupTests
{
	private readonly Mock<IChannelRepository> _channelRepositoryMock = new();
	private readonly Mock<IRepositoryService> _repositoryServiceMock = new();
	private readonly List<ChannelEntity> _channels = [];
	private readonly List<Query<ChannelEntity>> _queries = [];

	public CatalogLookupTests()
	{
		_channelRepositoryMock.Setup(x => x.GetListAsync(
				It.IsAny<Expression<Func<ChannelEntity, ChannelInfo>>>(),
				It.IsAny<Query<ChannelEntity>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Expression<Func<ChannelEntity, ChannelInfo>> projection, Query<ChannelEntity> query, CancellationToken token) =>
			{
				_queries.Add(query);

				return [.. _channels.Where(query.Where!.Compile()).Select(projection.Compile())];
			});

		_repositoryServiceMock.SetupGet(x => x.Channels).Returns(_channelRepositoryMock.Object);
	}

	[TestMethod]
	public async Task LoadChannelsAsyncShouldReadTheChannelsInChunks()
	{
		int count = (CatalogLookup.ChannelChunkSize * 2) + 1;
		List<string> identifiers = [.. Enumerable.Range(0, count).Select(index => $"Channel{index:D4}.de")];
		_channels.AddRange(identifiers.Select(CreateChannel));

		Dictionary<string, ChannelInfo> channels = await CatalogLookup
			.LoadChannelsAsync(_repositoryServiceMock.Object, identifiers, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(3, _queries, "Every chunk is one query, so SQLite never sees too many parameters.");
		Assert.HasCount(count, channels);
	}

	[TestMethod]
	public async Task LoadChannelsAsyncShouldNotQueryWithoutChannels()
	{
		Dictionary<string, ChannelInfo> channels = await CatalogLookup
			.LoadChannelsAsync(_repositoryServiceMock.Object, [], TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsEmpty(channels);
		Assert.IsEmpty(_queries);
	}

	[TestMethod]
	public async Task LookupShouldIgnoreTheCaseOfTheIdentifier()
	{
		_channels.Add(CreateChannel("DasErste.de"));

		Dictionary<string, ChannelInfo> channels = await CatalogLookup
			.LoadChannelsAsync(_repositoryServiceMock.Object, ["DasErste.de"], TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.AreEqual("Name of DasErste.de", CatalogLookup.Lookup(channels, "daserste.DE")?.Name);
	}

	[TestMethod]
	[DataRow(null, DisplayName = "a row that names no channel")]
	[DataRow("Unknown.de", DisplayName = "a channel the catalog does not know")]
	public void LookupShouldReturnNullForAnUnknownChannel(string? channel)
	{
		Dictionary<string, ChannelInfo> channels = new(StringComparer.OrdinalIgnoreCase)
		{
			["DasErste.de"] = new ChannelInfo("DasErste.de", "Das Erste", "DE")
		};

		Assert.IsNull(CatalogLookup.Lookup(channels, channel));
	}

	private static ChannelEntity CreateChannel(string channel) => new()
	{
		Channel = channel,
		Name = $"Name of {channel}",
		Country = "DE",
		AltNames = [],
		Categories = [],
		Owners = [],
		IsNsfw = false
	};

	public TestContext TestContext { get; set; }
}
