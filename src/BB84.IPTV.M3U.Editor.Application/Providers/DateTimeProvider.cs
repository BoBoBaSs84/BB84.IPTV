// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;

namespace BB84.IPTV.M3U.Editor.Application.Providers;

/// <summary>
/// The implementation for the date time provider contract.
/// </summary>
/// <inheritdoc cref="IDateTimeProvider"/>
[ExcludeFromCodeCoverage(Justification = "This class is a simple wrapper around the System.DateTime class.")]
internal sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime Now => DateTime.Now;

	public TimeSpan TimeOfDay => DateTime.Now.TimeOfDay;

	public DateTime Today => DateTime.Today;

	public DateTime UtcNow => DateTime.UtcNow;

	public DateTime MaxValue => DateTime.MaxValue;

	public DateTime MinValue => DateTime.MinValue;
}
