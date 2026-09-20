// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents a channel of the iptv-org catalog as it is shown in the catalog list.
/// </summary>
public sealed class CatalogChannelResponse
{
	/// <summary>
	/// Gets or initializes the iptv-org channel identifier.
	/// </summary>
	public required string Channel { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org feed identifier of the stream, <see langword="null"/> if unknown.
	/// </summary>
	public string? Feed { get; init; }

	/// <summary>
	/// Gets or initializes the name of the channel.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Gets or initializes the country code (ISO 3166-1 alpha-2) the channel is based in.
	/// </summary>
	public required string Country { get; init; }

	/// <summary>
	/// Gets or initializes the languages (ISO 639-3) of the feeds of the channel.
	/// </summary>
	public IReadOnlyList<string> Languages { get; init; } = [];

	/// <summary>
	/// Gets or initializes the categories the channel belongs to.
	/// </summary>
	public IReadOnlyList<string> Categories { get; init; } = [];

	/// <summary>
	/// Gets or initializes a value indicating whether the channel is marked as NSFW.
	/// </summary>
	public bool IsNsfw { get; init; }

	/// <summary>
	/// Gets or initializes the stream URL, <see langword="null"/> if the catalog holds no stream for the channel.
	/// </summary>
	public string? StreamUrl { get; init; }

	/// <summary>
	/// Gets or initializes the quality of the stream, e.g. <c>1080p</c>.
	/// </summary>
	public string? Quality { get; init; }

	/// <summary>
	/// Gets or initializes the logo URL, <see langword="null"/> if the catalog holds no logo for the channel.
	/// </summary>
	public string? LogoUrl { get; init; }
}
