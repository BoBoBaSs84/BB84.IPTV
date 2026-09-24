// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

namespace BB84.IPTV.M3U.Editor.Tests.Extensions;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
	[TestMethod]
	public void RegisteredServicesShouldPassTheValidationOfTheDevelopmentEnvironment()
	{
		IHostEnvironment environment = CreateEnvironment(Environments.Development);
		IServiceCollection services = new ServiceCollection()
			.AddSingleton(environment)
			.RegisterServices(environment);

		// The host builder validates like this in development, so a captive dependency such as a
		// scoped service in a singleton one fails the start of the application.
		ServiceProviderOptions options = new() { ValidateScopes = true, ValidateOnBuild = true };

		using ServiceProvider provider = services.BuildServiceProvider(options);

		Assert.IsNotNull(provider.GetRequiredService<ILogoService>());
	}

	/// <summary>
	/// Creates the host environment the services are registered for.
	/// </summary>
	/// <param name="environmentName">The name of the host environment.</param>
	/// <returns>The host environment instance.</returns>
	private static IHostEnvironment CreateEnvironment(string environmentName)
	{
		Mock<IHostEnvironment> environmentMock = new Mock<IHostEnvironment>().SetupAllProperties();
		environmentMock.Object.ApplicationName = "BB84.IPTV.Test";
		environmentMock.Object.EnvironmentName = environmentName;

		return environmentMock.Object;
	}
}
