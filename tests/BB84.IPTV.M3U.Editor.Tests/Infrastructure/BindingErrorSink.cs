// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using Avalonia.Logging;

namespace BB84.IPTV.M3U.Editor.Tests.Infrastructure;

/// <summary>
/// Collects the binding messages Avalonia logs while a view is shown.
/// </summary>
/// <remarks>
/// A binding that does not resolve is only a log message at runtime, never an exception, so the
/// messages are the only way a test can see a broken binding.
/// </remarks>
internal sealed class BindingErrorSink : ILogSink, IDisposable
{
	private readonly ILogSink? _previousSink;
	private readonly List<string> _messages = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="BindingErrorSink"/> class and collects from now on.
	/// </summary>
	public BindingErrorSink()
	{
		_previousSink = Logger.Sink;
		Logger.Sink = this;
	}

	/// <summary>
	/// Gets the collected messages.
	/// </summary>
	public IReadOnlyList<string> Messages
		=> _messages;

	/// <inheritdoc/>
	public bool IsEnabled(LogEventLevel level, string area)
		=> level >= LogEventLevel.Warning && area == LogArea.Binding;

	/// <inheritdoc/>
	public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
		=> Collect(level, area, source, messageTemplate, []);

	/// <inheritdoc/>
	public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
		=> Collect(level, area, source, messageTemplate, propertyValues);

	/// <summary>
	/// Restores the sink that was active before.
	/// </summary>
	public void Dispose()
		=> Logger.Sink = _previousSink;

	private void Collect(LogEventLevel level, string area, object? source, string messageTemplate, object?[] propertyValues)
	{
		if (!IsEnabled(level, area))
			return;

		string message = messageTemplate;

		// The template holds {Placeholder} names, not indexes, so the values are appended instead.
		if (propertyValues.Length > 0)
		{
			string values = string.Join(", ", propertyValues.Select(value => value?.ToString() ?? "null"));
			message = string.Create(CultureInfo.InvariantCulture, $"{messageTemplate} [{values}]");
		}

		_messages.Add(string.Create(CultureInfo.InvariantCulture, $"{level} {area}: {message} (source: {source?.GetType().Name ?? "unknown"})"));
	}
}