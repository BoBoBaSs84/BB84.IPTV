using BB84.IPTV.M3U.Editor.Application.Installer;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Installer;

[TestClass]
public class DependencyInjectionInstallerTests
{
	[TestMethod]
	public void RegisterApplicationServicesTest()
	{
		ServiceCollection services = new();

		services.RegisterApplicationServices();

		Assert.HasCount(20, services);
	}
}
