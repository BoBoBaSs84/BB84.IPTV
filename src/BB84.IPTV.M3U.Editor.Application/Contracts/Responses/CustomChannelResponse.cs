// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents a stored user defined channel.
/// </summary>
public sealed class CustomChannelResponse
{
	/// <summary>
	/// Gets or initializes the identifier of the custom channel.
	/// </summary>
	public int Id { get; init; }

	/// <summary>
	/// Gets or initializes the name of the channel.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Gets or initializes the link to the media source, e.g. the stream URL.
	/// </summary>
	public required string Url { get; init; }

	/// <summary>
	/// Gets or initializes the group title (<c>group-title</c>).
	/// </summary>
	public string? GroupTitle { get; init; }

	/// <summary>
	/// Gets or initializes the TV guide identifier (<c>tvg-id</c>).
	/// </summary>
	public string? TvgId { get; init; }

	/// <summary>
	/// Gets or initializes the logo URL or file path (<c>tvg-logo</c>).
	/// </summary>
	public string? TvgLogo { get; init; }
}
