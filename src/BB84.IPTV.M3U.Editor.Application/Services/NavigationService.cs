using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// The navigation service class.
/// </summary>
/// <param name="viewModelFactory">The function for creating a specific view model.</param>
internal sealed class NavigationService(Func<Type, ViewModelBase> viewModelFactory) : NotifiableObject, INavigationService
{
	private ViewModelBase _currentView = default!;

	public ViewModelBase CurrentView
	{
		get => _currentView;
		private set => SetProperty(ref _currentView, value);
	}

	public void NavigateTo<T>() where T : ViewModelBase, INavigateable
		=> CurrentView = viewModelFactory.Invoke(typeof(T));
}
