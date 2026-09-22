// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Picks the logo a channel is shown and exported with.
/// </summary>
/// <remarks>
/// iptv-org knows several logos per channel; the one of the feed that is used wins, then the one
/// without special tags, then the format that is shown best, then the larger image.
/// </remarks>
internal static class LogoSelector
{
	/// <summary>
	/// The formats in the order they are preferred, everything else counts as the last one.
	/// </summary>
	private static readonly string[] FormatOrder = ["PNG", "WEBP", "APNG", "JPEG", "JPG", "SVG", "GIF", "AVIF"];

	/// <summary>
	/// The tags of a logo that is meant for a special background and is therefore not preferred.
	/// </summary>
	private static readonly string[] SpecialTags = ["dark", "light", "white", "black", "horizontal", "vertical", "square"];

	/// <summary>
	/// Picks the logo of a channel.
	/// </summary>
	/// <param name="logos">The logos the catalog knows for the channel.</param>
	/// <param name="feed">The feed the entry belongs to, if any.</param>
	/// <returns>The logo to use, or <see langword="null"/> if there is none.</returns>
	internal static LogoEntity? Select(IEnumerable<LogoEntity> logos, string? feed = null)
		=> logos
			.OrderBy(logo => MatchesFeed(logo, feed) ? 0 : 1)
			.ThenBy(logo => logo.Feed is null ? 0 : 1)
			.ThenBy(GetTagRank)
			.ThenBy(GetFormatRank)
			.ThenByDescending(logo => logo.Width * logo.Height)
			.FirstOrDefault();

	private static bool MatchesFeed(LogoEntity logo, string? feed)
		=> feed is not null && feed.Equals(logo.Feed, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// A logo without a special tag is the one that fits anywhere.
	/// </summary>
	private static int GetTagRank(LogoEntity logo)
		=> logo.Tags is null || logo.Tags.Count is 0
			? 0
			: logo.Tags.Any(tag => SpecialTags.Contains(tag, StringComparer.OrdinalIgnoreCase)) ? 2 : 1;

	private static int GetFormatRank(LogoEntity logo)
	{
		if (string.IsNullOrWhiteSpace(logo.Format))
			return FormatOrder.Length;

		int index = Array.FindIndex(FormatOrder, format => format.Equals(logo.Format.Trim(), StringComparison.OrdinalIgnoreCase));

		return index < 0 ? FormatOrder.Length : index;
	}
}