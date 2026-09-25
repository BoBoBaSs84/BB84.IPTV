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
	public async Task GetBlocklistsAsyncShouldPublishErrorOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Throws<Exception>();

		IEnumerable<BlocklistRequest> result = await _sut
			.GetBlocklistsAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetBlocklistsAsyncShouldPublishErrorWhenNotSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.InternalServerError));

		IEnumerable<BlocklistRequest> result = await _sut
			.GetBlocklistsAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task GetBlocklistsAsyncShouldReturnDataWhenSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.OK, Resources.BlocklistJson));

		IEnumerable<BlocklistRequest> result = await _sut
			.GetBlocklistsAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiItemsReceived, Times.Once());
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Never);
	}
}
