using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Installer;

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

		Assert.HasCount(38, services);
	}
}
