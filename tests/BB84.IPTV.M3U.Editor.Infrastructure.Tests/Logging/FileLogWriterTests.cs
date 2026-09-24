// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Infrastructure.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Logging;

[TestClass]
public sealed class FileLogWriterTests
{
	private const string ApplicationName = "BB84.IPTV.Test";

	[TestMethod]
	public void FileShouldBeNamedAfterTheApplicationAndTheDay()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter sut = new(directory.Path, ApplicationName);
		DateOnly date = new(2026, 9, 24);

		string filePath = sut.GetFilePath(date);

		Assert.AreEqual(Path.Combine(directory.Path, $"{ApplicationName}-20260924.log"), filePath);
	}

	[TestMethod]
	public void WriteShouldCreateTheDirectoryAndAppendTheEntries()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter sut = new(directory.Path, ApplicationName);

		sut.Write("the first entry");
		sut.Write("the second entry");

		string[] lines = TestLogDirectory.ReadAllLines(sut.GetFilePath(DateOnly.FromDateTime(DateTime.Now)));
		Assert.HasCount(2, lines);
		Assert.AreEqual("the first entry", lines[0]);
		Assert.AreEqual("the second entry", lines[1]);
	}

	[TestMethod]
	public void WriteShouldKeepOnlyTheNewestFiles()
	{
		using TestLogDirectory directory = new();
		_ = Directory.CreateDirectory(directory.Path);

		DateOnly today = DateOnly.FromDateTime(DateTime.Now);
		for (int day = 1; day <= 10; day++)
		{
			DateOnly date = today.AddDays(-day);
			File.WriteAllText(
				Path.Combine(directory.Path, $"{ApplicationName}-{date.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.log"),
				"an old entry");
		}

		using FileLogWriter sut = new(directory.Path, ApplicationName);
		sut.Write("the new entry");

		string[] files = Directory.GetFiles(directory.Path, $"{ApplicationName}-*.log");
		Assert.HasCount(FileLogWriter.RetainedFileCount, files);
		Assert.Contains(sut.GetFilePath(today), files);
		Assert.DoesNotContain(
			Path.Combine(directory.Path, $"{ApplicationName}-{today.AddDays(-10).ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.log"),
			files);
	}

	[TestMethod]
	public void WriteShouldNotThrowWhenTheDirectoryCannotBeUsed()
	{
		using TestLogDirectory directory = new();
		string filePath = Path.Combine(directory.Path, "blocked");
		_ = Directory.CreateDirectory(directory.Path);
		File.WriteAllText(filePath, "a file where the directory should be");

		// The log directory is a file here, so neither the directory nor the file can be created.
		using FileLogWriter sut = new(filePath, ApplicationName);

		sut.Write("the entry that goes nowhere");
	}
}
