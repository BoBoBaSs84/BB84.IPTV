// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
