using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Services;

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

		_repositoryServiceMock.Setup(x => x.Channels).Returns(new Mock<IChannelRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Categories).Returns(new Mock<ICategoryRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Countries).Returns(new Mock<ICountryRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Languages).Returns(new Mock<ILanguageRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Feeds).Returns(new Mock<IFeedRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Guides).Returns(new Mock<IGuideRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Logos).Returns(new Mock<ILogoRepository>().Object);
		_repositoryServiceMock.Setup(x => x.Streams).Returns(new Mock<IStreamRepository>().Object);
		_repositoryServiceMock.Setup(x => x.CommitChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

		_webServiceMock.Setup(x => x.GetCategoriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<CategoryRequest>());
		_webServiceMock.Setup(x => x.GetCountriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<CountryRequest>());
		_webServiceMock.Setup(x => x.GetLanguagesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<LanguageRequest>());
		_webServiceMock.Setup(x => x.GetChannelsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<ChannelRequest>());
		_webServiceMock.Setup(x => x.GetFeedsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<FeedRequest>());
		_webServiceMock.Setup(x => x.GetGuidesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<GuideRequest>());
		_webServiceMock.Setup(x => x.GetLogosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<LogoRequest>());
		_webServiceMock.Setup(x => x.GetStreamsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<StreamRequest>());

		_serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
		_serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
		_serviceProviderMock.Setup(x => x.GetService(typeof(IRepositoryService))).Returns(_repositoryServiceMock.Object);
		_serviceProviderMock.Setup(x => x.GetService(typeof(IWebService))).Returns(_webServiceMock.Object);

		return new(_eventServiceMock.Object, _serviceScopeFactoryMock.Object);
	}
}
