using BB84.IPTV.M3U.Editor.Infrastructure.Common;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Common;

[TestClass]
public class ApplicationPathsTests
{
	[TestMethod]
	public void DataDirectoryTest()
	{
		string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		Assert.AreEqual(Path.Combine(localAppData, AssemblyInformation.Product), ApplicationPaths.DataDirectory);
	}

	[TestMethod]
	public void DatabaseFilePathTest()
	{
		Assert.AreEqual(ApplicationPaths.DataDirectory, Path.GetDirectoryName(ApplicationPaths.DatabaseFilePath));
		Assert.AreEqual($"{AssemblyInformation.Product}.db", Path.GetFileName(ApplicationPaths.DatabaseFilePath));
	}

	[TestMethod]
	public void SettingsFilePathTest()
	{
		Assert.AreEqual(ApplicationPaths.DataDirectory, Path.GetDirectoryName(ApplicationPaths.SettingsFilePath));
		Assert.AreEqual($"{AssemblyInformation.Product}.ini", Path.GetFileName(ApplicationPaths.SettingsFilePath));
	}

	[TestMethod]
	public void LogDirectoryTest()
	{
		Assert.AreEqual(ApplicationPaths.DataDirectory, Path.GetDirectoryName(ApplicationPaths.LogDirectory));
	}
}