// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

/// <summary>
/// Provides string helpers for user-facing messages.
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// Replaces the placeholders of a localized message format with the <paramref name="args"/>,
	/// using the current culture.
	/// </summary>
	/// <param name="format">The message format, usually a localized resource.</param>
	/// <param name="args">The values for the placeholders.</param>
	/// <returns>The formatted message.</returns>
	public static string FormatMessage(this string format, params object[] args)
		=> string.Format(CultureInfo.CurrentCulture, format, args);

	/// <summary>
	/// Trims the <paramref name="value"/>, a blank value becomes <see langword="null"/>.
	/// </summary>
	/// <param name="value">The value to trim.</param>
	/// <returns>The trimmed value, or <see langword="null"/> if it is blank.</returns>
	internal static string? TrimToNull(this string? value)
		=> string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}