// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class LogoServiceTests
{
	[TestMethod]
	public async Task CacheLogoAsyncShouldDownloadTheLogoThatWasAskedFor()
	{
		// The second logo of the channel is the tagged one, which a run of the cache never picks.
		string? path = await _sut
			.CacheLogoAsync(2, TestContext.CancellationToken)
			.ConfigureAwait(false);

		LogoEntity logo = _logos[1];

		Assert.AreEqual(Path.Combine("logos", "DasErste.de", "DasErste.de-2.png"), path);
		Assert.AreEqual(path, logo.LocalPath);
		Assert.AreEqual("\"tag\"", logo.ETag);
		Assert.IsNotNull(logo.DownloadedAt);
		_downloadServiceMock.Verify(x => x.DownloadAsync("https://logo.example/ard-dark.png", null, It.IsAny<CancellationToken>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LogoCacheChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task CacheLogoAsyncShouldKeepTheFileNamesOfTheOtherLogosOfTheChannel()
	{
		_ = await _sut.CacheLogosAsync(cancellationToken: TestContext.CancellationToken).ConfigureAwait(false);

		string? path = await _sut
			.CacheLogoAsync(2, TestContext.CancellationToken)
			.ConfigureAwait(false);

		// The logo that is picked for the channel keeps its file, the tagged one gets its own.
		Assert.AreEqual(Path.Combine("logos", "DasErste.de", "DasErste.de-1.png"), _logos[0].LocalPath);
		Assert.AreEqual(Path.Combine("logos", "DasErste.de", "DasErste.de-2.png"), path);
	}

	[TestMethod]
	public async Task CacheLogoAsyncShouldReportNothingWhenTheLogoIsGone()
	{
		string? path = await _sut
			.CacheLogoAsync(99, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsNull(path);
		_downloadServiceMock.Verify(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LogoCacheChangedEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task CacheLogoAsyncShouldReportWhenTheLogoCannotBeStored()
	{
		_logoStoreServiceMock.Setup(x => x.SaveAsync("DasErste.de", It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new IOException("There is not enough space on the disk."));

		string? path = await _sut
			.CacheLogoAsync(1, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsNull(path);
		Assert.IsNull(_logos[0].LocalPath);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<WarningOccuredEvent>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LogoCacheChangedEvent>()), Times.Never);
	}
}
