// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.Concurrent;

using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class LogoServiceTests
{
	private readonly Mock<ILogoRepository> _logoRepositoryMock = new();
	private readonly Mock<IDownloadService> _downloadServiceMock = new();
	private readonly Mock<ILogoStoreService> _logoStoreServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	/// <summary>
	/// The files the store holds; a batch writes them from several threads, like the real store.
	/// </summary>
	private readonly ConcurrentDictionary<string, byte> _filesOnDisk = new(StringComparer.OrdinalIgnoreCase);
	private readonly List<LogoEntity> _logos = [];
	private readonly LogoService _sut;

	public LogoServiceTests()
	{
		_logos.AddRange(
		[
			CreateLogo(1, "DasErste.de", null, "https://logo.example/ard.png", "PNG", tags: []),
			CreateLogo(2, "DasErste.de", null, "https://logo.example/ard-dark.png", "PNG", tags: ["dark"]),
			CreateLogo(3, "ZDF.de", null, "https://logo.example/zdf.svg", "SVG", tags: [])
		]);

		_logoRepositoryMock.Setup(x => x.GetListAsync(It.IsAny<Query<LogoEntity>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((Query<LogoEntity> query, CancellationToken token) => query.Where is null
				? _logos
				: [.. _logos.Where(query.Where.Compile())]);

		_logoStoreServiceMock.Setup(x => x.Exists(It.IsAny<string>()))
			.Returns((string? path) => path is not null && _filesOnDisk.ContainsKey(path));

		_logoStoreServiceMock.Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((string channel, string fileName, byte[] content, CancellationToken token) =>
			{
				string path = Path.Combine("logos", channel, fileName);
				_ = _filesOnDisk.TryAdd(path, 0);
				return path;
			});

		_downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new LogoDownloadResponse { Content = [1, 2, 3], ETag = "\"tag\"", ContentType = "image/png" });

		_sut = new LogoService(
			CreateScopeFactory(),
			_downloadServiceMock.Object,
			_logoStoreServiceMock.Object,
			new ProviderService(),
			_eventServiceMock.Object);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldDownloadOneLogoPerChannel()
	{
		int downloaded = await _sut.CacheLogosAsync().ConfigureAwait(false);

		Assert.AreEqual(2, downloaded);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/ard.png", null, It.IsAny<CancellationToken>()), Times.Once);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/zdf.svg", null, It.IsAny<CancellationToken>()), Times.Once);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/ard-dark.png", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldStoreWhatItDownloaded()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);

		LogoEntity logo = _logos[0];

		Assert.AreEqual(Path.Combine("logos", "DasErste.de", "DasErste.de.png"), logo.LocalPath);
		Assert.AreEqual("\"tag\"", logo.ETag);
		Assert.AreEqual(3, logo.FileSize);
		Assert.IsNotNull(logo.ContentHash);
		Assert.IsNotNull(logo.DownloadedAt);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldSkipWhatIsAlreadyCached()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		_downloadServiceMock.Invocations.Clear();

		int downloaded = await _sut.CacheLogosAsync().ConfigureAwait(false);

		Assert.AreEqual(0, downloaded);
		_downloadServiceMock.Verify(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldAskWithTheEntityTagWhenRefreshing()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		_downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new LogoDownloadResponse { NotModified = true, ETag = "\"tag\"" });

		int downloaded = await _sut.CacheLogosAsync(new LogoCacheRequest { RefreshCached = true }).ConfigureAwait(false);

		Assert.AreEqual(0, downloaded);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/ard.png", "\"tag\"", It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldKeepWhatItHasWhenCancelled()
	{
		using CancellationTokenSource cancellation = new();
		_downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			{
				// The first logo is downloaded, then the run is stopped.
				cancellation.Cancel();
				return new LogoDownloadResponse { Content = [1], ContentType = "image/png" };
			});

		int downloaded = await _sut.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = 1 }, cancellation.Token).ConfigureAwait(false);

		Assert.AreEqual(1, downloaded);
		Assert.HasCount(1, _filesOnDisk);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldDownloadABatchAtOnce()
	{
		int running = 0;
		int highWaterMark = 0;

		_downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.Returns(async () =>
			{
				highWaterMark = Math.Max(highWaterMark, Interlocked.Increment(ref running));
				await Task.Delay(20).ConfigureAwait(false);
				_ = Interlocked.Decrement(ref running);

				return new LogoDownloadResponse { Content = [1], ContentType = "image/png" };
			});

		int downloaded = await _sut
			.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = 4 })
			.ConfigureAwait(false);

		Assert.AreEqual(2, downloaded);
		Assert.AreEqual(2, highWaterMark, "Both logos of the batch are downloaded at the same time.");
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldDownloadOneAfterAnotherWhenAsked()
	{
		int running = 0;
		int highWaterMark = 0;

		_downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.Returns(async () =>
			{
				highWaterMark = Math.Max(highWaterMark, Interlocked.Increment(ref running));
				await Task.Delay(20).ConfigureAwait(false);
				_ = Interlocked.Decrement(ref running);

				return new LogoDownloadResponse { Content = [1], ContentType = "image/png" };
			});

		_ = await _sut
			.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = 1 })
			.ConfigureAwait(false);

		Assert.AreEqual(1, highWaterMark);
	}

	[TestMethod]
	[DataRow(0, 1)]
	[DataRow(-3, 1)]
	[DataRow(8, 8)]
	[DataRow(99, LogoCacheRequest.MaxParallelLimit)]
	public void MaxParallelDownloadsShouldStayWithinTheAllowedRange(int value, int expected)
		=> Assert.AreEqual(expected, new LogoCacheRequest { MaxParallelDownloads = value }.MaxParallelDownloads);

	[TestMethod]
	public async Task CacheLogosAsyncShouldReportTheProgressPerBatch()
	{
		List<LogoCacheProgressEvent> progress = [];
		_eventServiceMock.Setup(x => x.Publish(It.IsAny<LogoCacheProgressEvent>()))
			.Callback((LogoCacheProgressEvent @event) => progress.Add(@event));

		// One logo at a time, so every logo reports on its own.
		_ = await _sut.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = 1 }).ConfigureAwait(false);

		Assert.HasCount(2, progress);
		Assert.AreEqual(50, progress[0].ProgressPercentage);
		Assert.AreEqual(100, progress[1].ProgressPercentage);
		Assert.AreEqual(2, progress[1].TotalCount);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LogoCacheChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldReportOnceForABatchThatHoldsEverything()
	{
		_ = await _sut.CacheLogosAsync(new LogoCacheRequest { MaxParallelDownloads = 4 }).ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LogoCacheProgressEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldSkipAChannelThatIsNotAsked()
	{
		int downloaded = await _sut
			.CacheLogosAsync(new LogoCacheRequest { Channels = ["ZDF.de"] })
			.ConfigureAwait(false);

		Assert.AreEqual(1, downloaded);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/zdf.svg", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task GetStatusAsyncShouldCountOneLogoPerChannel()
	{
		LogoCacheStatusResponse before = await _sut.GetStatusAsync().ConfigureAwait(false);
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		LogoCacheStatusResponse after = await _sut.GetStatusAsync().ConfigureAwait(false);

		Assert.AreEqual(2, before.TotalCount);
		Assert.AreEqual(0, before.CachedCount);
		Assert.AreEqual(2, before.MissingCount);
		Assert.AreEqual(2, after.CachedCount);
		Assert.AreEqual(6, after.CachedBytes);
	}

	[TestMethod]
	public async Task GetPathsByUrlAsyncShouldOnlyReportFilesThatAreThere()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		_ = _filesOnDisk.TryRemove(_logos[2].LocalPath!, out _);

		IReadOnlyDictionary<string, string> paths = await _sut.GetPathsByUrlAsync().ConfigureAwait(false);

		Assert.HasCount(1, paths);
		Assert.IsTrue(paths.ContainsKey("https://logo.example/ard.png"));
	}

	[TestMethod]
	public async Task GetLocalPathAsyncShouldReturnTheCachedLogoOfTheChannel()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);

		string? path = await _sut.GetLocalPathAsync("DasErste.de").ConfigureAwait(false);
		string? unknown = await _sut.GetLocalPathAsync("Unknown.de").ConfigureAwait(false);

		Assert.AreEqual(Path.Combine("logos", "DasErste.de", "DasErste.de.png"), path);
		Assert.IsNull(unknown);
	}

	private static LogoEntity CreateLogo(int id, string channel, string? feed, string url, string format, string[] tags) => new()
	{
		Id = id,
		Channel = channel,
		Feed = feed,
		Url = url,
		Format = format,
		Tags = tags,
		Width = 512,
		Height = 512
	};

	private IServiceScopeFactory CreateScopeFactory()
	{
		Mock<IRepositoryService> repositoryServiceMock = new();
		repositoryServiceMock.SetupGet(x => x.Logos).Returns(_logoRepositoryMock.Object);
		repositoryServiceMock.Setup(x => x.CommitChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

		Mock<IServiceProvider> serviceProviderMock = new();
		serviceProviderMock.Setup(x => x.GetService(typeof(IRepositoryService))).Returns(repositoryServiceMock.Object);

		Mock<IServiceScope> scopeMock = new();
		scopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

		Mock<IServiceScopeFactory> scopeFactoryMock = new();
		scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

		return scopeFactoryMock.Object;
	}
}