// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed class DownloadServiceTests
{
	private const string Url = "https://logo.example/ard.png";

	private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
	private readonly Mock<ILoggerService<DownloadService>> _loggerServiceMock = new();

	[TestMethod]
	public async Task DownloadAsyncShouldSkipTheLogoWhenTheRequestRanIntoTheTimeout()
	{
		// What HttpClient throws when its own timeout elapsed: a cancellation nobody asked for.
		DownloadService sut = CreateSut(new TaskCanceledException(
			"The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing.",
			new TimeoutException()));

		LogoDownloadResponse? response = await sut.DownloadAsync(Url).ConfigureAwait(false);

		Assert.IsNull(response, "A timeout is a logo that could not be downloaded, not the end of the run.");
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), Url, It.IsAny<Exception>()), Times.Once);
	}

	[TestMethod]
	public async Task DownloadAsyncShouldPassOnTheCancellationOfTheRun()
	{
		DownloadService sut = CreateSut(new OperationCanceledException());
		using CancellationTokenSource cancellation = new();

		await cancellation.CancelAsync().ConfigureAwait(false);

		// HttpClient wraps it, the run is stopped by whichever cancellation reaches the caller.
		_ = await Assert.ThrowsAsync<OperationCanceledException>(
			() => sut.DownloadAsync(Url, cancellationToken: cancellation.Token)).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task DownloadAsyncShouldSkipTheLogoWhenTheHostAnswersWithAnError()
	{
		DownloadService sut = CreateSut(new HttpResponseMessage(HttpStatusCode.Forbidden));

		LogoDownloadResponse? response = await sut.DownloadAsync(Url).ConfigureAwait(false);

		Assert.IsNull(response);
		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), Url, null), Times.Once);
	}

	[TestMethod]
	public async Task DownloadAsyncShouldBringWhatTheHostSent()
	{
		HttpResponseMessage message = new(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) };
		message.Content.Headers.ContentType = new("image/png");

		DownloadService sut = CreateSut(message);

		LogoDownloadResponse? response = await sut.DownloadAsync(Url).ConfigureAwait(false);

		Assert.IsNotNull(response);
		Assert.HasCount(3, response.Content);
		Assert.AreEqual("image/png", response.ContentType);
	}

	private DownloadService CreateSut(HttpResponseMessage response)
		=> CreateSut(handler => handler
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(response));

	private DownloadService CreateSut(Exception exception)
		=> CreateSut(handler => handler
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ThrowsAsync(exception));

	private DownloadService CreateSut(Action<Moq.Protected.IProtectedMock<HttpMessageHandler>> setup)
	{
		Mock<HttpMessageHandler> handlerMock = new();
		setup(handlerMock.Protected());

		_httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
			.Returns(() => new HttpClient(handlerMock.Object, false));

		return new DownloadService(_httpClientFactoryMock.Object, _loggerServiceMock.Object);
	}
}
