// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents a channel of the iptv-org catalog in the catalog list.
/// </summary>
/// <param name="channel">The catalog channel to show.</param>
public sealed class CatalogChannelViewModel(CatalogChannelResponse channel) : ViewModelBase
{
	/// <summary>
	/// Gets the catalog channel behind the list entry.
	/// </summary>
	public CatalogChannelResponse Channel { get; } = channel;

	/// <summary>
	/// Gets the name of the channel.
	/// </summary>
	public string Name
		=> Channel.Name;

	/// <summary>
	/// Gets the TV guide identifier (<c>tvg-id</c>) the channel is added with.
	/// </summary>
	public string TvgId
		=> Channel.ToTvgId();

	/// <summary>
	/// Gets the country code (ISO 3166-1 alpha-2) the channel is based in.
	/// </summary>
	public string Country
		=> Channel.Country;

	/// <summary>
	/// Gets the languages of the channel, as one line.
	/// </summary>
	public string Languages
		=> string.Join(", ", Channel.Languages);

	/// <summary>
	/// Gets the categories of the channel, as one line.
	/// </summary>
	public string Categories
		=> string.Join(", ", Channel.Categories);

	/// <summary>
	/// Gets the quality of the stream, e.g. <c>1080p</c>.
	/// </summary>
	public string? Quality
		=> Channel.Quality;

	/// <summary>
	/// Gets the stream URL, empty if the catalog holds no stream for the channel.
	/// </summary>
	public string StreamUrl
		=> Channel.StreamUrl ?? string.Empty;

	/// <summary>
	/// Indicates whether the channel is marked as NSFW.
	/// </summary>
	public bool IsNsfw
		=> Channel.IsNsfw;
}
