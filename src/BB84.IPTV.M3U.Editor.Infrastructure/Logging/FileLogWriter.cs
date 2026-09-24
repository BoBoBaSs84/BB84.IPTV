// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Logging;

/// <summary>
/// Writes the log entries to a daily file in the log directory of the application.
/// </summary>
/// <remarks>
/// One file per day, named <c>&lt;application&gt;-&lt;yyyyMMdd&gt;.log</c>, of which the newest
/// <see cref="RetainedFileCount"/> are kept. A file system that does not take the entry is no
/// reason to end the application, so a failed write is dropped instead of thrown.
/// </remarks>
internal sealed class FileLogWriter : IDisposable
{
	/// <summary>
	/// The number of daily files that are kept, the older ones are deleted.
	/// </summary>
	internal const int RetainedFileCount = 7;

	private readonly Lock _lock = new();
	private readonly string _directory;
	private readonly string _applicationName;

	private StreamWriter? _writer;
	private DateOnly _writerDate;

	/// <summary>
	/// Initializes a new instance of the <see cref="FileLogWriter"/> class.
	/// </summary>
	/// <param name="directory">The directory that holds the log files.</param>
	/// <param name="applicationName">The name the log files start with.</param>
	internal FileLogWriter(string directory, string applicationName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directory);
		ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

		_directory = directory;
		_applicationName = applicationName;
	}

	/// <summary>
	/// Appends <paramref name="entry"/> to the file of the current day.
	/// </summary>
	/// <param name="entry">The entry to append, without the trailing line break.</param>
	internal void Write(string entry)
	{
		lock (_lock)
		{
			try
			{
				GetWriter(DateOnly.FromDateTime(DateTime.Now)).WriteLine(entry);
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
			{
				// A log entry is never worth an exception, so a file that cannot be written is dropped.
			}
		}
	}

	/// <summary>
	/// Returns the full path of the log file of <paramref name="date"/>.
	/// </summary>
	/// <param name="date">The day the file holds.</param>
	/// <returns>The full path of the log file.</returns>
	internal string GetFilePath(DateOnly date)
		=> Path.Combine(_directory, $"{_applicationName}-{date.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.log");

	/// <inheritdoc/>
	public void Dispose()
	{
		lock (_lock)
		{
			_writer?.Dispose();
			_writer = null;
		}
	}

	/// <summary>
	/// Returns the writer of <paramref name="date"/>, switching the file when the day changed.
	/// </summary>
	/// <param name="date">The day the entry belongs to.</param>
	/// <returns>The writer of the file of that day.</returns>
	private StreamWriter GetWriter(DateOnly date)
	{
		if (_writer is not null && _writerDate == date)
			return _writer;

		_writer?.Dispose();
		_ = Directory.CreateDirectory(_directory);

		// Shared, so the file can be read while the application is writing it.
		FileStream stream = new(GetFilePath(date), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

		_writer = new StreamWriter(stream) { AutoFlush = true };
		_writerDate = date;

		RemoveOldFiles();

		return _writer;
	}

	/// <summary>
	/// Deletes the log files beyond the newest <see cref="RetainedFileCount"/> ones.
	/// </summary>
	/// <remarks>
	/// The file of the current day sorts last, so the file that is written stays.
	/// </remarks>
	private void RemoveOldFiles()
	{
		try
		{
			string[] files = Directory.GetFiles(_directory, $"{_applicationName}-*.log");

			if (files.Length <= RetainedFileCount)
				return;

			foreach (string file in files.OrderDescending(StringComparer.Ordinal).Skip(RetainedFileCount))
				File.Delete(file);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// The next run tries again, a file that is still open is kept.
		}
	}
}
