// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Domain.Extensions;

/// <summary>
/// The domain service collection extensions class.
/// </summary>
internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the required domain models to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The same service collection instance, so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterModels(this IServiceCollection services)
	{

		return services;
	}
}
