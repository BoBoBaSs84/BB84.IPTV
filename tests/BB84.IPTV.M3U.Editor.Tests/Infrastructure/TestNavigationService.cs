// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Tests.Infrastructure;

/// <summary>
/// Navigates between the view models the test provides.
/// </summary>
/// <remarks>
/// The navigation service of the application is internal, so the tests bring their own; it behaves
/// the same, which is all the data templates of <c>App.axaml</c> need.
/// </remarks>
/// <param name="viewModels">The view models to navigate to, by their type.</param>
internal sealed class TestNavigationService(IReadOnlyDictionary<Type, ViewModelBase> viewModels) : NotifiableObject, INavigationService
{
	private ViewModelBase _currentView = default!;

	/// <inheritdoc/>
	public ViewModelBase CurrentView
	{
		get => _currentView;
		private set => SetProperty(ref _currentView, value);
	}

	/// <inheritdoc/>
	public void NavigateTo<T>() where T : ViewModelBase, INavigateable
		=> CurrentView = viewModels.TryGetValue(typeof(T), out ViewModelBase? viewModel)
			? viewModel
			: throw new InvalidOperationException($"No view model for {typeof(T).Name}.");
}