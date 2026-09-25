using System.Net;
using System.Text;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Infrastructure.Tests.Common;

using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed partial class WebServiceTests
{
	private readonly WebService _sut;
	private Mock<IHttpClientFactory> _httpClientFactoryMock = default!;
	private Mock<ILogger<WebService>> _loggerMock = default!;
	private Mock<IEventService> _eventServiceMock = default!;
	private Mock<HttpMessageHandler> _httpMessageHandler = new();

	public WebServiceTests()
		=> _sut = CreateMockedInstance();

	private WebService CreateMockedInstance()
	{
		_httpClientFactoryMock = new();
		_loggerMock = new Mock<ILogger<WebService>>().WithLoggingEnabled();
		_eventServiceMock = new();

		return new(_httpClientFactoryMock.Object, _loggerMock.Object, _eventServiceMock.Object);
	}

	private HttpClient CreateMockedClient(HttpStatusCode statusCode, string? content = null)
	{
		_httpMessageHandler = new(MockBehavior.Strict);

		HttpResponseMessage responseMessage = new(statusCode)
		{
			Content = new StringContent(content ?? string.Empty, Encoding.UTF8, Constants.HttpClientMediaType)
		};

		_httpMessageHandler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage)
			.Verifiable();

		return new(_httpMessageHandler.Object) { BaseAddress = new Uri(Constants.HttpClientBaseAddress) };
	}
}
