using System.Net;

using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Properties;

using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

public sealed partial class WebServiceTests
{
	[TestMethod]
	public async Task GetCountriesAsyncShouldLogAndRaiseEventOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Throws<Exception>();

		IEnumerable<CountryRequest> result = await _sut
			.GetCountriesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, Exception?>>(), It.IsAny<Exception?>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetCountriesAsyncShouldLogAndNotifyOnNotSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.InternalServerError));

		IEnumerable<CountryRequest> result = await _sut
			.GetCountriesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Once);
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, Exception?>>(), It.IsAny<Exception?>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetCountriesAsyncShouldReturnDataWhenSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.OK, Resources.CountryJson));

		IEnumerable<CountryRequest> result = await _sut
			.GetCountriesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Exactly(2));
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, Exception?>>(), It.IsAny<Exception?>()), Times.Never);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Never);
	}
}
