using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

/// <summary>
/// The application service collection extensions class.
/// </summary>
[SuppressMessage("Style", "IDE0058", Justification = "Not relevant here, dependency injection.")]
internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the required settings and settings services to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterSettings(this IServiceCollection services)
	{
		services.AddSingleton<ApplicationSettings>();

		return services;
	}

	/// <summary>
	/// Registers the required application services to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterServices(this IServiceCollection services)
	{
		services.AddSingleton<IEventService, EventService>();
		services.AddSingleton<IDatabaseService, DatabaseService>();
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<IPlaylistService, PlaylistService>();
		services.AddSingleton<IProviderService, ProviderService>();
		services.AddSingleton<ISerializerService, SerializerService>();

		services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider
			=> viewModelType => (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));

		return services;
	}

	/// <summary>
	/// Registers the required view models to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterViewModels(this IServiceCollection services)
	{
		services.AddSingleton<AboutViewModel>();
		services.AddSingleton<DatabaseViewModel>();
		services.AddSingleton<MainViewModel>();
		services.AddSingleton<PlaylistViewModel>();
		services.AddSingleton<PlaylistsViewModel>();
		services.AddSingleton<SettingsViewModel>();

		return services;
	}
}
