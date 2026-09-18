using System.Net;
using System.Text;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Moq;
using Moq.Protected;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed partial class WebServiceTests
{
	private readonly WebService _sut;
	private Mock<IHttpClientFactory> _httpClientFactoryMock = default!;
	private Mock<ILoggerService<WebService>> _loggerServiceMock = default!;
	private Mock<IEventService> _eventServiceMock = default!;
	private Mock<HttpMessageHandler> _httpMessageHandler = new();

	public WebServiceTests()
		=> _sut = CreateMockedInstance();

	private WebService CreateMockedInstance()
	{
		_httpClientFactoryMock = new();
		_loggerServiceMock = new();
		_eventServiceMock = new();

		return new(_httpClientFactoryMock.Object, _loggerServiceMock.Object, _eventServiceMock.Object);
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
