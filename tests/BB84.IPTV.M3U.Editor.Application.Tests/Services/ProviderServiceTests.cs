using BB84.IPTV.M3U.Editor.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Services;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class ProviderServiceTests
{
	private readonly ProviderService _sut;

	public ProviderServiceTests()
		=> _sut = new ProviderService();

	[TestMethod]
	public void AllProvidersShouldReturnNotNull()
	{
		Assert.IsInstanceOfType<DateTimeProvider>(_sut.DateTime);
		Assert.IsInstanceOfType<DirectoryProvider>(_sut.Directory);
		Assert.IsInstanceOfType<EnvironmentProvider>(_sut.Environment);
		Assert.IsInstanceOfType<FileProvider>(_sut.File);
		Assert.IsInstanceOfType<PathProvider>(_sut.Path);
	}
}
