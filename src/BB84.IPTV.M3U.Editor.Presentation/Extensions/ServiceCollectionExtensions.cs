using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Presentation.Controls;
using BB84.IPTV.M3U.Editor.Presentation.Services;
using BB84.IPTV.M3U.Editor.Presentation.Views;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Presentation.Extensions;

/// <summary>
/// The presentation service collection extensions.
/// </summary>
internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the required WPF views to the <paramref name="services"/> collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterControls(this IServiceCollection services)
	{
		services.AddSingleton<AboutControl>();
		services.AddSingleton<DatabaseControl>();
		services.AddSingleton<PlaylistControl>();

		return services;
	}

	/// <summary>
	/// Registers the required services to the <paramref name="services"/> collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterServices(this IServiceCollection services)
	{
		services.AddSingleton<IFileDialogService, FileDialogService>();
		services.AddTransient<IUserService, UserService>();
		services.AddSingleton<INotificationService, NotificationService>();

		return services;
	}

	/// <summary>
	/// Registers the required windows to the <paramref name="services"/> collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterWindows(this IServiceCollection services)
	{
		services.AddSingleton<MainWindow>();

		return services;
	}
}
