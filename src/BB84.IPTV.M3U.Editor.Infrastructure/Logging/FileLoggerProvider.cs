// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Logging;

/// <summary>
/// Provides the loggers that write into the log file of the application.
/// </summary>
/// <remarks>
/// All categories share one <see cref="FileLogWriter"/>, because the entries of an application that
/// runs on a desktop belong in one file, and the category is not part of an entry.
/// </remarks>
internal sealed class FileLoggerProvider : ILoggerProvider
{
	private readonly FileLogWriter _writer;
	private readonly GeneralSettings _settings;

	/// <summary>
	/// Initializes a new instance of the <see cref="FileLoggerProvider"/> class.
	/// </summary>
	/// <param name="pathService">The service that provides the log directory.</param>
	/// <param name="applicationName">The name the log files start with.</param>
	/// <param name="settings">The general settings that say what is logged.</param>
	public FileLoggerProvider(IPathService pathService, string applicationName, GeneralSettings settings)
	{
		ArgumentNullException.ThrowIfNull(pathService);
		ArgumentNullException.ThrowIfNull(settings);

		_writer = new FileLogWriter(pathService.LogDirectory, applicationName);
		_settings = settings;
	}

	/// <inheritdoc/>
	public ILogger CreateLogger(string categoryName)
		=> new FileLogger(_writer, _settings);

	/// <inheritdoc/>
	public void Dispose()
		=> _writer.Dispose();
}
