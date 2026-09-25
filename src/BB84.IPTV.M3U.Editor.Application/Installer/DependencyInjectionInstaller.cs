// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Installer;

/// <summary>
/// The application dependency injection installer class.
/// </summary>
[SuppressMessage("Style", "IDE0058", Justification = "Not relevant here.")]
public static class DependencyInjectionInstaller
{
	/// <summary>
	/// Registers the application services to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The same service collection instance, so that multiple calls can be chained.</returns>
	public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
	{
		services.RegisterSettings()
			.RegisterServices()
			.RegisterViewModels();

		return services;
	}
}
