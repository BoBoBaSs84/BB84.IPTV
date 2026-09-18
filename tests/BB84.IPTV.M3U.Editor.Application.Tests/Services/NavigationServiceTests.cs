using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class NavigationServiceTests
{
	[TestMethod]
	public void NavigateToShouldNavigateToTheSpecifiedViewModel()
	{
		Func<Type, ViewModelBase> viewModelFactory = new(type => new TestViewModel());
		NavigationService navigationService = new(viewModelFactory);

		navigationService.NavigateTo<TestViewModel>();

		Assert.IsNotNull(navigationService.CurrentView);
		Assert.IsInstanceOfType<TestViewModel>(navigationService.CurrentView);
	}

	private sealed class TestViewModel : ViewModelBase, INavigateable
	{ }
}
