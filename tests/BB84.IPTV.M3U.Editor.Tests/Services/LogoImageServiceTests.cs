// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Services;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Services;

[TestClass]
public sealed class LogoImageServiceTests
{
	private readonly Mock<ILogoService> _logoServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly Mock<IProviderService> _providerServiceMock = new();
	private readonly Mock<IFileProvider> _fileProviderMock = new();
	private readonly Mock<IPathProvider> _pathProviderMock = new();

	public LogoImageServiceTests()
	{
		// Every path is taken as a cached file, so the cache is filled without touching the disk.
		_fileProviderMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
		_pathProviderMock.Setup(x => x.GetExtension(It.IsAny<string>())).Returns(".png");

		_providerServiceMock.SetupGet(x => x.File).Returns(_fileProviderMock.Object);
		_providerServiceMock.SetupGet(x => x.Path).Returns(_pathProviderMock.Object);

		_logoServiceMock.Setup(x => x.GetPathsByUrlAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new Dictionary<string, string>());
	}

	[TestMethod]
	public void GetImageShouldKeepAtMostTheCapacityOfImages()
	{
		using LogoImageService sut = CreateService();

		for (int index = 0; index < LogoImageService.CacheCapacity + 50; index++)
			_ = sut.GetImage($"logo-{index}.png");

		Assert.AreEqual(LogoImageService.CacheCapacity, sut.CachedImageCount);
	}

	[TestMethod]
	public void GetImageShouldCacheOnePathOnce()
	{
		using LogoImageService sut = CreateService();

		_ = sut.GetImage("logo.png");
		_ = sut.GetImage("logo.png");

		Assert.AreEqual(1, sut.CachedImageCount);
	}

	[TestMethod]
	public void GetImageShouldNotCacheALogoThatIsNotThere()
	{
		_fileProviderMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

		using LogoImageService sut = CreateService();

		Assert.IsNull(sut.GetImage("logo.png"));
		Assert.AreEqual(0, sut.CachedImageCount);
	}

	[TestMethod]
	public void GetImageShouldReturnNothingWithoutALogo()
	{
		using LogoImageService sut = CreateService();

		Assert.IsNull(sut.GetImage(null));
		Assert.IsNull(sut.GetImage(string.Empty));
		Assert.IsNull(sut.GetImage(" "));
		Assert.AreEqual(0, sut.CachedImageCount);
	}

	[TestMethod]
	public async Task RefreshShouldForgetTheLoadedImages()
	{
		using LogoImageService sut = CreateService();

		_ = sut.GetImage("logo.png");
		Assert.AreEqual(1, sut.CachedImageCount);

		await sut.RefreshAsync().ConfigureAwait(false);

		Assert.AreEqual(0, sut.CachedImageCount);
	}

	[TestMethod]
	public void DisposeShouldStopListeningToTheCacheAndForgetTheImages()
	{
		LogoImageService sut = CreateService();

		_ = sut.GetImage("logo.png");

		sut.Dispose();

		Assert.AreEqual(0, sut.CachedImageCount);
		_eventServiceMock.Verify(x => x.Unsubscribe(It.IsAny<Action<LogoCacheChangedEvent>>()), Times.Once);
	}

	private LogoImageService CreateService()
		=> new(_logoServiceMock.Object, _eventServiceMock.Object, _providerServiceMock.Object);
}
