using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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

using Serilog;
using Serilog.Events;

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
			Directory.CreateDirectory(ApplicationPaths.DataDirectory);
			string connectionString = $"Data Source={ApplicationPaths.DatabaseFilePath}";
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
				builder.AddSerilog(CreateFileLogger(environment), dispose: true);
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

		services.AddScoped<IDownloadService, DownloadService>();

		services.AddScoped<IRepositoryService, RepositoryService>();
		services.AddScoped<IWebService, WebService>();

		return services;
	}

	/// <summary>
	/// Creates a cross-platform logger that writes to daily rolling files in the application log directory.
	/// </summary>
	/// <remarks>
	/// The minimum level is left at <see cref="LogEventLevel.Verbose"/>, filtering is done by the
	/// <see cref="ILoggingBuilder"/> minimum level.
	/// </remarks>
	/// <param name="environment">The host environment instance to use.</param>
	/// <returns>The configured Serilog logger.</returns>
	private static Serilog.Core.Logger CreateFileLogger(IHostEnvironment environment)
	{
		string filePath = Path.Combine(ApplicationPaths.LogDirectory, $"{environment.ApplicationName}-.log");

		return new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.File(filePath, formatProvider: CultureInfo.InvariantCulture, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
			.CreateLogger();
	}
}
