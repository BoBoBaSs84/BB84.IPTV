// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

/// <summary>
/// Helps a test work with a mocked logger.
/// </summary>
/// <remarks>
/// A source generated entry is written inside an <see cref="ILogger.IsEnabled(LogLevel)"/> guard and
/// a loose mock answers that with <see langword="false"/>, so a mock a test asserts on must be told
/// that logging is enabled.
/// </remarks>
internal static class LoggerMockExtensions
{
	/// <summary>
	/// Makes the mocked logger answer every level as enabled.
	/// </summary>
	/// <typeparam name="TLogger">The type of the mocked logger.</typeparam>
	/// <param name="mock">The mock to set up.</param>
	/// <returns>The same mock, so the call can be chained.</returns>
	internal static Mock<TLogger> WithLoggingEnabled<TLogger>(this Mock<TLogger> mock) where TLogger : class, ILogger
	{
		mock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
		return mock;
	}

	/// <summary>
	/// Verifies that the entry with the given level and event id was written.
	/// </summary>
	/// <typeparam name="TLogger">The type of the mocked logger.</typeparam>
	/// <param name="mock">The mock to verify.</param>
	/// <param name="logLevel">The level the entry is expected at.</param>
	/// <param name="eventId">The event id the entry is expected with.</param>
	/// <param name="times">How often the entry is expected.</param>
	/// <param name="because">Says what the entry is good for.</param>
	[SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging",
		Justification = "The call is an expression tree Moq matches against, nothing is logged here.")]
	internal static void VerifyLogged<TLogger>(this Mock<TLogger> mock, LogLevel logLevel, int eventId, Times times,
		string? because = null) where TLogger : class, ILogger
		=> mock.Verify(
			logger => logger.Log(
				logLevel,
				It.Is<EventId>(id => id.Id == eventId),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception?>(),
				(Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
			times,
			because ?? $"The entry with the event id {eventId} was not written as expected.");
}
