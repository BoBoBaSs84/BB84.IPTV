// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Installer;
using BB84.IPTV.M3U.Editor.Domain.Installer;
using BB84.IPTV.M3U.Editor.Infrastructure.Installer;
using BB84.IPTV.M3U.Editor.Services;
using BB84.IPTV.M3U.Editor.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BB84.IPTV.M3U.Editor.Extensions;

/// <summary>
/// The <see cref="IServiceCollection"/> extensions class.
/// </summary>
[SuppressMessage("Style", "IDE0058", Justification = "Not relevant here, dependency injection.")]
internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the all the required services to the <paramref name="services"/> collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <param name="environment">The host environment instance to use.</param>
	/// <returns>The enriched service collection.</returns>
	internal static IServiceCollection RegisterServices(this IServiceCollection services, IHostEnvironment environment)
	{
		services.RegisterApplicationServices()
			.RegisterDomainServices()
			.RegisterInfrastructureServices(environment)
			.RegisterServices()
			.RegisterWindows();

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
		services.AddSingleton<LogoImageService>();
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