// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

/// <summary>
/// Deletes the cached file of a logo the catalog no longer knows, and refuses everything else.
/// </summary>
[TestClass]
public sealed class LogoStoreServiceTests
{
	private readonly Mock<IPathService> _pathServiceMock = new();
	private readonly LogoStoreService _sut;
	private readonly string _logoDirectory;

	public LogoStoreServiceTests()
	{
		_logoDirectory = Path.Combine(Path.GetTempPath(), $"bb84-iptv-logos-{Guid.NewGuid():N}");
		_ = Directory.CreateDirectory(_logoDirectory);

		_pathServiceMock.SetupGet(x => x.LogoDirectory).Returns(_logoDirectory);

		// The providers of the application are internal, so the real file system is wired by hand.
		Mock<IFileProvider> fileProviderMock = new();
		fileProviderMock.Setup(x => x.Exists(It.IsAny<string>())).Returns((string path) => File.Exists(path));
		fileProviderMock.Setup(x => x.Delete(It.IsAny<string>())).Callback((string path) => File.Delete(path));

		Mock<IPathProvider> pathProviderMock = new();
		pathProviderMock.Setup(x => x.GetFullPath(It.IsAny<string>())).Returns((string path) => Path.GetFullPath(path));

		Mock<IProviderService> providerServiceMock = new();
		providerServiceMock.SetupGet(x => x.File).Returns(fileProviderMock.Object);
		providerServiceMock.SetupGet(x => x.Path).Returns(pathProviderMock.Object);

		_sut = new LogoStoreService(_pathServiceMock.Object, providerServiceMock.Object);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if (Directory.Exists(_logoDirectory))
			Directory.Delete(_logoDirectory, true);
	}

	[TestMethod]
	public void DeleteShouldRemoveAFileOfTheLogoDirectory()
	{
		string channelDirectory = Path.Combine(_logoDirectory, "DasErste.de");
		_ = Directory.CreateDirectory(channelDirectory);
		string filePath = Path.Combine(channelDirectory, "ard.png");
		File.WriteAllBytes(filePath, [1, 2, 3]);

		bool deleted = _sut.Delete(filePath);

		Assert.IsTrue(deleted);
		Assert.IsFalse(File.Exists(filePath));
	}

	[TestMethod]
	public void DeleteShouldRefuseAFileOutsideTheLogoDirectory()
	{
		string filePath = Path.Combine(Path.GetTempPath(), $"bb84-iptv-other-{Guid.NewGuid():N}.png");
		File.WriteAllBytes(filePath, [1, 2, 3]);

		try
		{
			// A stored path can point into a directory the settings named earlier.
			bool deleted = _sut.Delete(filePath);

			Assert.IsFalse(deleted);
			Assert.IsTrue(File.Exists(filePath));
		}
		finally
		{
			File.Delete(filePath);
		}
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public void DeleteShouldDoNothingWithoutAPath(string? path)
		=> Assert.IsFalse(_sut.Delete(path));

	[TestMethod]
	public void DeleteShouldDoNothingWhenTheFileIsAlreadyGone()
		=> Assert.IsFalse(_sut.Delete(Path.Combine(_logoDirectory, "gone.png")));
}
