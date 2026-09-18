using BB84.IPTV.M3U.Editor.Domain.Installer;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Domain.Tests.Installer;

[TestClass]
public sealed class DependencyInjectionInstallerTests
{
	[TestMethod]
	public void RegisterDomainServicesTest()
	{
		ServiceCollection services = new();

		services.RegisterDomainServices();

		Assert.AreEqual(0, services.Count);
	}
}
