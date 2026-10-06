// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

/// <remarks>
/// The repositories run the queries of the service in memory, so a test sees which rows the filter
/// keeps and in which order. Only ASCII text is used, because the case folding of .NET and SQLite
/// differ for other letters; the SQLite tests of the infrastructure cover the real database.
/// </remarks>
[TestClass]
public sealed partial class GuideServiceTests
{
	private readonly Mock<IGuideRepository> _guideRepositoryMock = new();
	private readonly Mock<IChannelRepository> _channelRepositoryMock = new();
	private readonly List<GuideEntity> _guides = [];
	private readonly List<ChannelEntity> _channels = [];

	/// <summary>
	/// The guide queries the service ran, so a test can tell what it read and how it read it.
	/// </summary>
	private readonly List<Query<GuideEntity>> _guideQueries = [];
	private int _countQueries;
	private readonly GuideService _sut;

	public GuideServiceTests()
	{
		_channels.AddRange(
		[
			CreateChannel("DasErste.de", "Das Erste"),
			CreateChannel("ZDF.de", "Zweites Programm")
		]);

		_guides.AddRange(
		[
			CreateGuide(1, "DasErste.de", null, "hoerzu.de", "ard", "ARD", "de"),
			CreateGuide(2, "DasErste.de", "SD", "tvtoday.de", "ard-1", "Das Erste", "de"),
			CreateGuide(3, "ZDF.de", null, "hoerzu.de", "zdf", "ZDF", "de"),
			CreateGuide(4, "Unknown.de", null, "hoerzu.de", "unknown", "Unknown station", "de"),
			CreateGuide(5, null, null, "hoerzu.de", "orphan", "Orphan", "en")
		]);

		_guideRepositoryMock.Setup(x => x.GetListAsync(It.IsAny<Query<GuideEntity>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Query<GuideEntity> query, CancellationToken token) =>
			{
				_guideQueries.Add(query);

				IEnumerable<GuideEntity> guides = query.Where is null ? _guides : _guides.Where(query.Where.Compile());

				if (query.OrderBy is not null)
					guides = query.OrderBy(guides.AsQueryable());

				if (query.Skip is int skip)
					guides = guides.Skip(skip);

				if (query.Take is int take)
					guides = guides.Take(take);

				return [.. guides];
			});

		_guideRepositoryMock.Setup(x => x.CountAsync(It.IsAny<Query<GuideEntity>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Query<GuideEntity> query, CancellationToken token) =>
			{
				_countQueries++;

				return query.Where is null ? _guides.Count : _guides.Count(query.Where.Compile());
			});

		_channelRepositoryMock.Setup(x => x.GetListAsync(
				It.IsAny<Expression<Func<ChannelEntity, string>>>(),
				It.IsAny<Query<ChannelEntity>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Expression<Func<ChannelEntity, string>> projection, Query<ChannelEntity> query, CancellationToken token)
				=> [.. Filter(query).Select(projection.Compile())]);

		_channelRepositoryMock.Setup(x => x.GetListAsync(
				It.IsAny<Expression<Func<ChannelEntity, ChannelInfo>>>(),
				It.IsAny<Query<ChannelEntity>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Expression<Func<ChannelEntity, ChannelInfo>> projection, Query<ChannelEntity> query, CancellationToken token)
				=> [.. Filter(query).Select(projection.Compile())]);

		_sut = new GuideService(
			CreateScopeFactory(),
			Mock.Of<IPlaylistService>(),
			Mock.Of<IChannelsXmlSerializer>(),
			Mock.Of<IProviderService>());
	}

	private IEnumerable<ChannelEntity> Filter(Query<ChannelEntity> query)
		=> query.Where is null ? _channels : _channels.Where(query.Where.Compile());

	private static ChannelEntity CreateChannel(string channel, string name) => new()
	{
		Channel = channel,
		Name = name,
		Country = "DE",
		AltNames = [],
		Categories = [],
		Owners = [],
		IsNsfw = false
	};

	private static GuideEntity CreateGuide(int id, string? channel, string? feed, string site, string siteId, string siteName, string lang) => new()
	{
		Id = id,
		Channel = channel,
		Feed = feed,
		Site = site,
		SiteId = siteId,
		SiteName = siteName,
		Lang = lang
	};

	private IServiceScopeFactory CreateScopeFactory()
	{
		Mock<IRepositoryService> repositoryServiceMock = new();
		repositoryServiceMock.SetupGet(x => x.Guides).Returns(_guideRepositoryMock.Object);
		repositoryServiceMock.SetupGet(x => x.Channels).Returns(_channelRepositoryMock.Object);

		Mock<IServiceProvider> serviceProviderMock = new();
		serviceProviderMock.Setup(x => x.GetService(typeof(IRepositoryService))).Returns(repositoryServiceMock.Object);

		Mock<IServiceScope> scopeMock = new();
		scopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

		Mock<IServiceScopeFactory> scopeFactoryMock = new();
		scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

		return scopeFactoryMock.Object;
	}

	public TestContext TestContext { get; set; }
}
