// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed class PathServiceTests
{
	[TestMethod]
	public void EmptySettingsShouldUseTheDefaultPaths()
	{
		PathService sut = new(new ApplicationSettings());

		Assert.AreEqual(ApplicationPaths.DataDirectory, sut.DataDirectory);
		Assert.AreEqual(ApplicationPaths.DatabaseFilePath, sut.DatabaseFilePath);
		Assert.AreEqual(ApplicationPaths.SettingsFilePath, sut.SettingsFilePath);
		Assert.AreEqual(ApplicationPaths.LogoDirectory, sut.LogoDirectory);
		Assert.AreEqual(ApplicationPaths.LogDirectory, sut.LogDirectory);
	}

	[TestMethod]
	public void ConfiguredDataDirectoryShouldHoldTheDatabaseTheLogosAndTheLogs()
	{
		string dataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-paths");
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = dataDirectory;

		PathService sut = new(settings);

		Assert.AreEqual(dataDirectory, sut.DataDirectory);
		Assert.AreEqual(Path.Combine(dataDirectory, ApplicationPaths.DatabaseFileName), sut.DatabaseFilePath);
		Assert.AreEqual(Path.Combine(dataDirectory, ApplicationPaths.LogoDirectoryName), sut.LogoDirectory);
		Assert.AreEqual(Path.Combine(dataDirectory, ApplicationPaths.LogDirectoryName), sut.LogDirectory);
	}

	[TestMethod]
	public void ConfiguredLogoAndLogDirectoryShouldWinOverTheDataDirectory()
	{
		string dataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-data");
		string logoDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-logos");
		string logDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-logs");
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = dataDirectory;
		settings.Paths.LogoDirectory = logoDirectory;
		settings.Paths.LogDirectory = logDirectory;

		PathService sut = new(settings);

		Assert.AreEqual(logoDirectory, sut.LogoDirectory);
		Assert.AreEqual(logDirectory, sut.LogDirectory);
	}

	[TestMethod]
	public void SettingsFilePathShouldStayTheDefaultOne()
	{
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-elsewhere");

		PathService sut = new(settings);

		Assert.AreEqual(ApplicationPaths.SettingsFilePath, sut.SettingsFilePath);
	}

	[TestMethod]
	[DataRow("relative/directory")]
	[DataRow("   ")]
	public void PathThatIsNotFullyQualifiedShouldFallBackToTheDefault(string path)
	{
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = path;

		PathService sut = new(settings);

		Assert.AreEqual(ApplicationPaths.DataDirectory, sut.DataDirectory);
	}

	[TestMethod]
	public void TrailingSeparatorShouldNotChangeTheResolvedPath()
	{
		string dataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-trailing");
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = dataDirectory + Path.DirectorySeparatorChar;

		PathService sut = new(settings);

		Assert.AreEqual(dataDirectory, sut.DataDirectory);
	}

	[TestMethod]
	public void HasPendingChangesShouldBeFalseWhileTheSettingsNameThePathsInUse()
	{
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-pending");

		PathService sut = new(settings);

		Assert.IsFalse(sut.HasPendingChanges());
	}

	[TestMethod]
	public void HasPendingChangesShouldBeTrueAfterAPathWasChanged()
	{
		ApplicationSettings settings = new();
		PathService sut = new(settings);

		settings.Paths.LogoDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-other-logos");

		Assert.IsTrue(sut.HasPendingChanges());
	}

	[TestMethod]
	public void HasPendingChangesShouldIgnoreAPathThatCannotBeUsed()
	{
		ApplicationSettings settings = new();
		PathService sut = new(settings);

		settings.Paths.DataDirectory = "relative/directory";

		Assert.IsFalse(sut.HasPendingChanges(), "An unusable path keeps the default, which is what is in use.");
	}

	[TestMethod]
	[DataRow("")]
	[DataRow(null)]
	public void EmptyPathShouldBeValidBecauseItStandsForTheDefault(string? path)
	{
		PathService sut = new(new ApplicationSettings());

		Assert.IsTrue(sut.IsValidDirectory(path));
	}

	[TestMethod]
	public void FullPathShouldBeValid()
	{
		PathService sut = new(new ApplicationSettings());

		Assert.IsTrue(sut.IsValidDirectory(Path.GetTempPath()));
	}

	[TestMethod]
	public void RelativePathShouldNotBeValid()
	{
		PathService sut = new(new ApplicationSettings());

		Assert.IsFalse(sut.IsValidDirectory(Path.Combine("some", "directory")));
	}
}