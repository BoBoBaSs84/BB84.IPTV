// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Extensions;
using BB84.IPTV.M3U.Editor.Infrastructure.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Extensions;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
	[TestMethod]
	public void ProductionShouldWriteToTheLogFile()
	{
		using ServiceProvider provider = BuildProvider(Environments.Production);

		ILoggerProvider[] loggerProviders = [.. provider.GetServices<ILoggerProvider>()];

		Assert.ContainsSingle(loggerProviders.OfType<FileLoggerProvider>());
	}

	[TestMethod]
	public void RegistrationShouldBringTheLoggerOfACategory()
	{
		using ServiceProvider provider = BuildProvider(Environments.Production);

		ILogger<FileLogger> logger = provider.GetRequiredService<ILogger<FileLogger>>();

		Assert.IsNotNull(logger, "Every class asks for its own logger, nothing wraps it any more.");
	}

	[TestMethod]
	public void DevelopmentShouldNotWriteToTheLogFile()
	{
		using ServiceProvider provider = BuildProvider(Environments.Development);

		ILoggerProvider[] loggerProviders = [.. provider.GetServices<ILoggerProvider>()];

		Assert.IsEmpty(loggerProviders.OfType<FileLoggerProvider>());
	}

	[TestMethod]
	public void TheDownloadClientShouldNameItself()
	{
		ServiceCollection services = new();

		using ServiceProvider provider = services.RegisterHttpClients().BuildServiceProvider();
		using HttpClient client = provider.GetRequiredService<IHttpClientFactory>()
			.CreateClient(Constants.DownloadClientName);

		// A host like Wikimedia answers a request without a User-Agent with 403 Forbidden.
		Assert.AreEqual(Constants.DownloadClientUserAgent, client.DefaultRequestHeaders.UserAgent.ToString());
	}

	[TestMethod]
	public void TheFileLoggerProviderShouldBeOwnedByTheContainer()
	{
		ServiceCollection services = BuildServices(Environments.Production);

		ServiceDescriptor descriptor = services
			.Single(service => service.ServiceType == typeof(ILoggerProvider));

		// Only a provider the container built is closed by it, and with it the log file it writes.
		Assert.IsNull(descriptor.ImplementationInstance, "An instance registration is never disposed by the container.");
		Assert.IsNotNull(descriptor.ImplementationFactory);
		Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
	}

	/// <summary>
	/// Builds the services the logger registration needs, for the given environment.
	/// </summary>
	/// <param name="environmentName">The name of the host environment.</param>
	/// <returns>The service provider with the registered logging.</returns>
	private static ServiceProvider BuildProvider(string environmentName)
		=> BuildServices(environmentName).BuildServiceProvider();

	/// <summary>
	/// Registers the services the logger registration needs, for the given environment.
	/// </summary>
	/// <param name="environmentName">The name of the host environment.</param>
	/// <returns>The service collection with the registered logging.</returns>
	private static ServiceCollection BuildServices(string environmentName)
	{
		Mock<IHostEnvironment> environmentMock = new Mock<IHostEnvironment>().SetupAllProperties();
		environmentMock.Object.ApplicationName = "BB84.IPTV.Test";
		environmentMock.Object.EnvironmentName = environmentName;

		ServiceCollection services = new();

		_ = services.AddSingleton(new ApplicationSettings())
			.RegisterPathService()
			.RegisterLoggerService(environmentMock.Object);

		return services;
	}
}
