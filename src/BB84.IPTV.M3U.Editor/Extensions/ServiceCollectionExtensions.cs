using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Installer;
using BB84.IPTV.M3U.Editor.Domain.Installer;
using BB84.IPTV.M3U.Editor.Infrastructure.Installer;
using BB84.IPTV.M3U.Editor.Presentation.Installer;

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
			.RegisterPresentationServices();

		return services;
	}
}
