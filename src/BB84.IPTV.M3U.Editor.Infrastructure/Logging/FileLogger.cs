// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;
using System.Text;

using BB84.IPTV.M3U.Editor.Application.Settings;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Logging;

/// <summary>
/// Writes the log entries of one category to the log file of the application.
/// </summary>
/// <remarks>
/// The settings are asked for every entry, so a level that is changed in the settings screen is
/// followed right away, and so is logging that is turned on or off. An entry names its event id
/// after the level, so the log says which of the entries of the application it is.
/// </remarks>
/// <param name="writer">The writer that holds the log file.</param>
/// <param name="settings">The general settings that say what is logged.</param>
internal sealed class FileLogger(FileLogWriter writer, GeneralSettings settings) : ILogger
{
	/// <summary>
	/// The format of the timestamp of an entry, e.g. <c>2026-09-24 11:50:14.017 +02:00</c>.
	/// </summary>
	private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";

	/// <inheritdoc/>
	/// <remarks>
	/// Scopes are not written, so there is nothing to begin.
	/// </remarks>
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		=> null;

	/// <inheritdoc/>
	public bool IsEnabled(LogLevel logLevel)
		=> settings.EnableLogging
		&& logLevel is not LogLevel.None
		&& settings.LogLevel is not LogLevel.None
		&& logLevel >= settings.LogLevel;

	/// <inheritdoc/>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		ArgumentNullException.ThrowIfNull(formatter);

		if (!IsEnabled(logLevel))
			return;

		string message = formatter(state, exception);

		if (message.Length is 0 && exception is null)
			return;

		StringBuilder builder = new();
		_ = builder.Append(DateTimeOffset.Now.ToString(TimestampFormat, CultureInfo.InvariantCulture))
			.Append(" [")
			.Append(GetLevelToken(logLevel))
			.Append("] ");

		// An entry without an id, e.g. one the framework writes, keeps the short form.
		if (eventId.Id is not 0)
		{
			_ = builder.Append('[')
				.Append(eventId.Id.ToString(CultureInfo.InvariantCulture))
				.Append("] ");
		}

		_ = builder.Append(message);

		if (exception is not null)
			_ = builder.Append(Environment.NewLine).Append(exception);

		writer.Write(builder.ToString());
	}

	/// <summary>
	/// Returns the three letter token an entry of <paramref name="logLevel"/> is marked with.
	/// </summary>
	/// <param name="logLevel">The level of the entry.</param>
	/// <returns>The token of that level.</returns>
	internal static string GetLevelToken(LogLevel logLevel)
		=> logLevel switch
		{
			LogLevel.Trace => "VRB",
			LogLevel.Debug => "DBG",
			LogLevel.Information => "INF",
			LogLevel.Warning => "WRN",
			LogLevel.Error => "ERR",
			LogLevel.Critical => "FTL",
			_ => "OFF",
		};
}
