// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net;

using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Properties;

using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

public sealed partial class WebServiceTests
{
	[TestMethod]
	public async Task GetGuidesAsyncShouldThrowOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Throws<Exception>();

		_ = await Assert.ThrowsExactlyAsync<Exception>(() => _sut.GetGuidesAsync(cancellationToken)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task GetGuidesAsyncShouldThrowWhenNotSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.InternalServerError));

		_ = await Assert.ThrowsExactlyAsync<HttpRequestException>(() => _sut.GetGuidesAsync(cancellationToken)).ConfigureAwait(false);

		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
	}

	[TestMethod]
	public async Task GetGuidesAsyncShouldReturnDataWhenSuccessful()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		_httpClientFactoryMock.Setup(x => x.CreateClient(Constants.HttpClientName))
			.Returns(CreateMockedClient(HttpStatusCode.OK, Resources.GuideJson));

		IEnumerable<GuideResponse> result = await _sut
			.GetGuidesAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.IsNotNull(result);
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiRequestSent, Times.Once());
		_loggerMock.VerifyLogged(LogLevel.Information, LogEvents.Web.ApiItemsReceived, Times.Once());
	}
}
