// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Logging;

/// <summary>
/// Provides a temporary log directory that is deleted with the test.
/// </summary>
internal sealed class TestLogDirectory : IDisposable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestLogDirectory"/> class.
	/// </summary>
	public TestLogDirectory()
		=> Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"bb84-iptv-logs-{Guid.NewGuid():N}");

	/// <summary>
	/// The directory the logger writes into, which does not exist yet.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Reads a log file the writer still holds open.
	/// </summary>
	/// <param name="filePath">The full path of the log file.</param>
	/// <returns>The content of the file.</returns>
	public static string ReadAllText(string filePath)
	{
		using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		using StreamReader reader = new(stream);

		return reader.ReadToEnd();
	}

	/// <summary>
	/// Reads the lines of a log file the writer still holds open.
	/// </summary>
	/// <param name="filePath">The full path of the log file.</param>
	/// <returns>The lines of the file, without the empty line at its end.</returns>
	public static string[] ReadAllLines(string filePath)
		=> ReadAllText(filePath).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

	/// <inheritdoc/>
	public void Dispose()
	{
		try
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, true);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// A directory that cannot be deleted is left to the operating system.
		}
	}
}
