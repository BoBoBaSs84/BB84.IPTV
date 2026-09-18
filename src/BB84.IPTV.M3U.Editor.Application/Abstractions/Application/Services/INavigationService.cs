using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Interfaces;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// The interface for the navigation service.
/// </summary>
public interface INavigationService : INotifiableObject
{
	/// <summary>
	/// The current view model.
	/// </summary>
	ViewModelBase CurrentView { get; }

	/// <summary>
	/// Navigates to the provided view model.
	/// </summary>
	/// <typeparam name="T">The view model to navigate to.</typeparam>
	void NavigateTo<T>() where T : ViewModelBase, INavigateable;
}
