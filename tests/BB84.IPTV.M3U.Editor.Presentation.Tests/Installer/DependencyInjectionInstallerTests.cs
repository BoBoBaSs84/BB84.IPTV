using BB84.IPTV.M3U.Editor.Presentation.Installer;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Presentation.Tests.Installer;

[TestClass]
public sealed class DependencyInjectionInstallerTests
{
	[TestMethod]
	public void RegisterPresentationServicesTest()
	{
		ServiceCollection services = new();

		services.RegisterPresentationServices();

		Assert.HasCount(7, services);
	}
}
