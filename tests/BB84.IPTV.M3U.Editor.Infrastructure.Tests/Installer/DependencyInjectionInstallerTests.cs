using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Installer;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Installer;

[TestClass]
public class DependencyInjectionInstallerTests
{
	[TestMethod]
	public void RegisterInfrastructureServicesTest()
	{
		Mock<IHostEnvironment> hostEnvironmentMock = new Mock<IHostEnvironment>()
			.SetupAllProperties();
		ServiceCollection services = new();

		services
			.AddSingleton<ApplicationSettings>()
			.RegisterInfrastructureServices(hostEnvironmentMock.Object);

		Assert.HasCount(42, services);
	}

	[TestMethod]
	public void RegisterInfrastructureServicesShouldRegisterThePathsOfTheSettingsFile()
	{
		Mock<IHostEnvironment> hostEnvironmentMock = new Mock<IHostEnvironment>()
			.SetupAllProperties();
		ApplicationSettings settings = new();
		ServiceCollection services = new();

		services
			.AddSingleton(settings)
			.RegisterInfrastructureServices(hostEnvironmentMock.Object);

		// The registration reads the settings file, so the paths are the ones the settings name.
		IPathService pathService = services.BuildServiceProvider().GetRequiredService<IPathService>();

		Assert.AreEqual(ApplicationPaths.SettingsFilePath, pathService.SettingsFilePath);
		Assert.AreEqual(new PathService(settings).DataDirectory, pathService.DataDirectory);
		Assert.IsFalse(pathService.HasPendingChanges());
	}
}
