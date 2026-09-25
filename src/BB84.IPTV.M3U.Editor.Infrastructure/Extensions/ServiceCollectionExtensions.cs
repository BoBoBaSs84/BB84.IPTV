// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Logging;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Extensions;

/// <summary>
/// The infrastructure service collection extensions.
/// </summary>
[SuppressMessage("Style", "IDE0058", Justification = "Not relevant here, dependency injection.")]
internal static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the path service to the service collection.
	/// </summary>
	/// <remarks>
	/// The settings file is read here already, because the database, the logger and the logo store
	/// are set up while the services are registered and need to know where they work. A file that
	/// cannot be read leaves the defaults in place, <see cref="SettingsService.LoadAsync"/> reports
	/// the error to the user once the application is up.
	/// </remarks>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterPathService(this IServiceCollection services)
	{
		ApplicationSettings settings = services
			.BuildServiceProvider()
			.GetRequiredService<ApplicationSettings>();

		LoadSettingsFile(settings);

		services.AddSingleton<IPathService>(new PathService(settings));

		return services;
	}

	/// <summary>
	/// Registers the database context to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <param name="environment">The host environment instance to use.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterDatabaseContext(this IServiceCollection services, IHostEnvironment environment)
	{
		ServiceProvider provider = services.BuildServiceProvider();
		DatabaseSettings settings = provider.GetRequiredService<ApplicationSettings>().Database;
		IPathService pathService = provider.GetRequiredService<IPathService>();

		services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
		{
			Directory.CreateDirectory(pathService.DataDirectory);
			string connectionString = $"Data Source={pathService.DatabaseFilePath}";
			options.UseSqlite(connectionString, options =>
			{
				options.CommandTimeout(settings.CommandTimeout);
				options.MaxBatchSize(settings.MaxBatchSize);
				options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
			});

			if (environment.IsDevelopment())
			{
				options.EnableDetailedErrors(true);
				options.EnableSensitiveDataLogging(true);
				options.LogTo(Console.WriteLine, LogLevel.Debug);
			}
			else
			{
				options.EnableDetailedErrors(false);
				options.EnableSensitiveDataLogging(false);
			}
		});

		return services;
	}

	/// <summary>
	/// Registers the logger service to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <param name="environment">The host environment instance to use.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterLoggerService(this IServiceCollection services, IHostEnvironment environment)
	{
		ServiceProvider provider = services.BuildServiceProvider();
		GeneralSettings settings = provider.GetRequiredService<ApplicationSettings>().General;
		IPathService pathService = provider.GetRequiredService<IPathService>();

		services.AddLogging(builder =>
		{
			builder.ClearProviders();

			if (environment.IsDevelopment())
			{
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddConsole();
			}

			if (environment.IsProduction())
			{
				builder.AddProvider(new FileLoggerProvider(pathService, environment.ApplicationName, settings));

				// The provider decides from the settings, so no rule may filter an entry before it.
				builder.AddFilter<FileLoggerProvider>(null, LogLevel.Trace);
			}
		});

		return services;
	}

	/// <summary>
	/// Registers the named http clients to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterHttpClients(this IServiceCollection services)
	{
		services.AddHttpClient(Constants.HttpClientName, configureClient =>
			configureClient.WithBaseAddress(Constants.HttpClientBaseAddress)
				.WithMediaType(Constants.HttpClientMediaType)
				.WithTimeout(TimeSpan.FromSeconds(30)));

		// The logos are hosted wherever their channel keeps them, so this client has no base address.
		services.AddHttpClient(Constants.DownloadClientName, configureClient =>
			configureClient.WithTimeout(TimeSpan.FromSeconds(30)));

		return services;
	}

	/// <summary>
	/// Registers the required infrastructure services to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterServices(this IServiceCollection services)
	{
		services.AddSingleton<IFileService, FileService>();
		services.AddSingleton<ILogoStoreService, LogoStoreService>();
		services.AddSingleton<ISettingsService, SettingsService>();
		services.AddSingleton<IDownloadService, DownloadService>();
		services.AddScoped<IRepositoryService, RepositoryService>();
		services.AddScoped<IWebService, WebService>();

		return services;
	}

	/// <summary>
	/// Reads the settings file into <paramref name="settings"/>, if it is there.
	/// </summary>
	/// <param name="settings">The settings instance the application shares.</param>
	private static void LoadSettingsFile(ApplicationSettings settings)
	{
		try
		{
			if (!File.Exists(ApplicationPaths.SettingsFilePath))
				return;

			string fileContent = File.ReadAllText(ApplicationPaths.SettingsFilePath);
			settings.Load(ApplicationSettings.Read(fileContent));
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
		{
			// The defaults are used, the settings service reports the error once the application is up.
		}
	}

}
