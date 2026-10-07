// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed partial class DatabaseServiceTests
{
	private readonly DatabaseService _sut;
	private Mock<IEventService> _eventServiceMock = default!;
	private Mock<IRepositoryService> _repositoryServiceMock = default!;
	private Mock<IWebService> _webServiceMock = default!;
	private Mock<IServiceScopeFactory> _serviceScopeFactoryMock = default!;
	private Mock<IServiceScope> _serviceScopeMock = default!;
	private Mock<IServiceProvider> _serviceProviderMock = default!;
	private Mock<IProviderService> _providerServiceMock = default!;
	private Mock<ILogoStoreService> _logoStoreServiceMock = default!;
	private Mock<IDateTimeProvider> _dateTimeProviderMock = default!;

	public DatabaseServiceTests()
		=> _sut = CreateDatabaseService();

	private DatabaseService CreateDatabaseService()
	{
		CancellationToken cancellationToken = It.IsAny<CancellationToken>();
		_eventServiceMock = new();
		_repositoryServiceMock = new();
		_webServiceMock = new();
		_serviceScopeFactoryMock = new();
		_serviceScopeMock = new();
		_serviceProviderMock = new();
		_providerServiceMock = new();
		_logoStoreServiceMock = new();
		_dateTimeProviderMock = new();

		_dateTimeProviderMock.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
		_providerServiceMock.SetupGet(x => x.DateTime).Returns(_dateTimeProviderMock.Object);

		Mock<ICatalogSyncRepository> catalogSyncRepositoryMock = new();
		catalogSyncRepositoryMock
			.Setup(x => x.GetListAsync(It.IsAny<Query<CatalogSyncEntity>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);
		_repositoryServiceMock.Setup(x => x.CatalogSyncs).Returns(catalogSyncRepositoryMock.Object);
		_repositoryServiceMock.Setup(x => x.Channels).Returns(new Mock<IChannelRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Categories).Returns(new Mock<ICategoryRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Countries).Returns(new Mock<ICountryRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Languages).Returns(new Mock<ILanguageRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Feeds).Returns(new Mock<IFeedRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Guides).Returns(new Mock<IGuideRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Logos).Returns(new Mock<ILogoRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Streams).Returns(new Mock<IStreamRepository>().Object);
		_repositoryServiceMock.Setup(x => x.CommitChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

		_webServiceMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<CategoryResponse>());
		_webServiceMock.Setup(x => x.GetCountriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<CountryResponse>());
		_webServiceMock.Setup(x => x.GetLanguagesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<LanguageResponse>());
		_webServiceMock.Setup(x => x.GetChannelsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<ChannelResponse>());
		_webServiceMock.Setup(x => x.GetFeedsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<FeedResponse>());
		_webServiceMock.Setup(x => x.GetGuidesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<GuideResponse>());
		_webServiceMock.Setup(x => x.GetLogosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<LogoResponse>());
		_webServiceMock.Setup(x => x.GetStreamsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<StreamResponse>());

		_serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
		_serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
		_serviceProviderMock.Setup(x => x.GetService(typeof(IRepositoryService))).Returns(_repositoryServiceMock.Object);
		_serviceProviderMock.Setup(x => x.GetService(typeof(IWebService))).Returns(_webServiceMock.Object);

		return new(
			_eventServiceMock.Object,
			_serviceScopeFactoryMock.Object,
			_providerServiceMock.Object,
			_logoStoreServiceMock.Object,
			new ApplicationSettings());
	}
}
