// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text.RegularExpressions;

using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Logging;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Logging;

[TestClass]
public sealed class FileLoggerTests
{
	private const string ApplicationName = "BB84.IPTV.Test";

	[TestMethod]
	public void DisabledLoggingShouldNotEvenCreateTheFile()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = false, LogLevel = LogLevel.Trace };
		FileLogger sut = new(writer, settings);

		sut.Log(LogLevel.Error, default, "the state", null, (state, exception) => state);

		Assert.IsFalse(Directory.Exists(directory.Path) && Directory.GetFiles(directory.Path).Length > 0);
	}

	[TestMethod]
	public void EntryBelowTheConfiguredLevelShouldNotBeWritten()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = true, LogLevel = LogLevel.Error };
		FileLogger sut = new(writer, settings);

		sut.Log(LogLevel.Debug, default, "a debug entry", null, (state, exception) => state);

		Assert.IsFalse(sut.IsEnabled(LogLevel.Debug));
		Assert.IsFalse(File.Exists(writer.GetFilePath(DateOnly.FromDateTime(DateTime.Now))));
	}

	[TestMethod]
	public void EntryShouldKeepTheFormatOfTheFormerLogger()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = true, LogLevel = LogLevel.Information };
		FileLogger sut = new(writer, settings);

		sut.Log(LogLevel.Information, default, "the message", null, (state, exception) => state);

		string[] lines = TestLogDirectory.ReadAllLines(writer.GetFilePath(DateOnly.FromDateTime(DateTime.Now)));
		Assert.HasCount(1, lines);
		Assert.IsTrue(
			Regex.IsMatch(lines[0], @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2} \[INF\] the message$"),
			$"'{lines[0]}' does not have the expected format.");
	}

	[TestMethod]
	public void ExceptionShouldFollowTheMessage()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = true, LogLevel = LogLevel.Error };
		FileLogger sut = new(writer, settings);
		InvalidOperationException exception = new("the failure");

		sut.Log(LogLevel.Error, default, "the message", exception, (state, error) => state);

		string fileContent = TestLogDirectory.ReadAllText(writer.GetFilePath(DateOnly.FromDateTime(DateTime.Now)));
		Assert.Contains("[ERR] the message", fileContent);
		Assert.Contains("System.InvalidOperationException: the failure", fileContent);
	}

	[TestMethod]
	public void ChangedSettingsShouldBeFollowedWithoutANewLogger()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = true, LogLevel = LogLevel.Error };
		FileLogger sut = new(writer, settings);

		sut.Log(LogLevel.Information, default, "before the change", null, (state, exception) => state);
		settings.LogLevel = LogLevel.Information;
		sut.Log(LogLevel.Information, default, "after the change", null, (state, exception) => state);
		settings.EnableLogging = false;
		sut.Log(LogLevel.Information, default, "after turning it off", null, (state, exception) => state);

		string fileContent = TestLogDirectory.ReadAllText(writer.GetFilePath(DateOnly.FromDateTime(DateTime.Now)));
		Assert.DoesNotContain("before the change", fileContent);
		Assert.Contains("after the change", fileContent);
		Assert.DoesNotContain("after turning it off", fileContent);
	}

	[TestMethod]
	[DataRow(LogLevel.Trace, "VRB")]
	[DataRow(LogLevel.Debug, "DBG")]
	[DataRow(LogLevel.Information, "INF")]
	[DataRow(LogLevel.Warning, "WRN")]
	[DataRow(LogLevel.Error, "ERR")]
	[DataRow(LogLevel.Critical, "FTL")]
	public void LevelTokenShouldMatchTheFormerLogger(LogLevel logLevel, string expected)
		=> Assert.AreEqual(expected, FileLogger.GetLevelToken(logLevel));

	[TestMethod]
	public void LevelNoneShouldSilenceTheLogger()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		GeneralSettings settings = new() { EnableLogging = true, LogLevel = LogLevel.None };
		FileLogger sut = new(writer, settings);

		Assert.IsFalse(sut.IsEnabled(LogLevel.Critical));
	}

	[TestMethod]
	public void ScopeShouldBeNothingToDispose()
	{
		using TestLogDirectory directory = new();
		using FileLogWriter writer = new(directory.Path, ApplicationName);
		FileLogger sut = new(writer, new GeneralSettings());

		Assert.IsNull(sut.BeginScope("the scope"));
	}
}
