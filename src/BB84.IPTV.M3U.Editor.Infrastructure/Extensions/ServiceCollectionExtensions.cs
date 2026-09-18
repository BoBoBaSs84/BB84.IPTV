using System.Diagnostics.CodeAnalysis;

using BB84.Extensions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
	/// Registers the database context to the service collection.
	/// </summary>
	/// <param name="services">The service collection to enrich.</param>
	/// <param name="environment">The host environment instance to use.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
	internal static IServiceCollection RegisterDatabaseContext(this IServiceCollection services, IHostEnvironment environment)
	{
		DatabaseSettings settings = services
			.BuildServiceProvider()
			.GetRequiredService<ApplicationSettings>().Database;

		services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
		{
			string connectionString = $"Data Source={AssemblyInformation.Product}.db";
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
		GeneralSettings settings = services
			.BuildServiceProvider()
			.GetRequiredService<ApplicationSettings>().General;

		services.TryAddSingleton(typeof(ILoggerService<>), typeof(LoggerService<>));

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
				builder.SetMinimumLevel(settings.LogLevel);
				builder.AddEventLog(settings => settings.SourceName = environment.ApplicationName);
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
		services.AddSingleton<ISettingsService, SettingsService>();

		services.AddScoped<IRepositoryService, RepositoryService>();
		services.AddScoped<IWebService, WebService>();

		return services;
	}
}
