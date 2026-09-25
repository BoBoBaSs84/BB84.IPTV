using System.Net;

using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Properties;

using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

public sealed partial class WebServiceTests
{
	[TestMethod]
	public async Task GetLanguagesAsyncShouldPublishErrorOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Throws<Exception>();

		IEnumerable<LanguageRequest> result = await _sut
			.GetLanguagesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetLanguagesAsyncShouldPublishErrorWhenNotSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.InternalServerError));

		IEnumerable<LanguageRequest> result = await _sut
			.GetLanguagesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetLanguagesAsyncShouldReturnDataWhenSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.OK, Resources.GuideJson));

		IEnumerable<LanguageRequest> result = await _sut
			.GetLanguagesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiItemsReceived, Times.Once());
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Never);
	}
}
