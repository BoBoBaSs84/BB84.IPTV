using BB84.IPTV.M3U.Editor.Application.ViewModels;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class AboutViewModelTests
{
	[TestMethod]
	public void ConstructorShouldSetPropertiesCorrect()
	{
		AboutViewModel? viewModel;

		viewModel = new();

		Assert.IsNotNull(viewModel);
		Assert.IsNotNull(viewModel.Title);
		Assert.IsNotNull(viewModel.Description);
		Assert.IsNotNull(viewModel.Version);
		Assert.IsNotNull(viewModel.Company);
		Assert.IsNotNull(viewModel.Copyright);
		Assert.IsNotNull(viewModel.FrameworkName);
		Assert.IsNotNull(viewModel.Repository);
	}
}
