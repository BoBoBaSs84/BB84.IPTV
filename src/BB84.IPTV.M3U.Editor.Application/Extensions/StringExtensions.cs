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

	/// <summary>
	/// Splits an identifier of the form <c>channel@feed</c>, as a <c>channels.xml</c> or the
	/// <c>tvg-id</c> of an entry holds it, into the channel and the feed.
	/// </summary>
	/// <remarks>
	/// Only the full form counts: both parts must be there. Everything after the first separator is
	/// the feed.
	/// </remarks>
	/// <param name="text">The text to split.</param>
	/// <param name="channel">The channel, empty if the text is not of the full form.</param>
	/// <param name="feed">The feed, empty if the text is not of the full form.</param>
	/// <returns><see langword="true"/> if the text names a channel and a feed.</returns>
	internal static bool TryParseChannelFeed(this string? text, out string channel, out string feed)
	{
		int separator = text?.IndexOf(FeedSeparator, StringComparison.Ordinal) ?? -1;

		if (text is null || separator <= 0 || separator >= text.Length - 1)
		{
			channel = string.Empty;
			feed = string.Empty;
			return false;
		}

		channel = text[..separator];
		feed = text[(separator + 1)..];
		return true;
	}

	/// <summary>
	/// The character that separates the channel from the feed in an iptv-org identifier.
	/// </summary>
	private const char FeedSeparator = '@';
}