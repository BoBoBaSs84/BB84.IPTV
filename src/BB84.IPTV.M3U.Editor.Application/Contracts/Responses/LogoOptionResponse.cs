// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents one logo of the iptv-org catalog, as it can be picked for an entry.
/// </summary>
/// <remarks>
/// The response holds both paths a <c>tvg-logo</c> can be set to: the URL the catalog knows and the
/// file the logo cache holds, which is only there while <see cref="IsCached"/> is set.
/// </remarks>
public sealed class LogoOptionResponse
{
	/// <summary>
	/// Gets or initializes the identifier of the stored logo, which names the row a single download
	/// works on.
	/// </summary>
	public required int Id { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org channel the logo belongs to.
	/// </summary>
	public required string Channel { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org feed the logo belongs to, not set for a logo that covers the
	/// channel as a whole.
	/// </summary>
	public string? Feed { get; init; }

	/// <summary>
	/// Gets or initializes the name the channel has in the catalog, <see langword="null"/> if the
	/// catalog does not know the channel.
	/// </summary>
	public string? ChannelName { get; init; }

	/// <summary>
	/// Gets or initializes the country the channel is based in (ISO 3166-1 alpha-2),
	/// <see langword="null"/> if the catalog does not know the channel.
	/// </summary>
	public string? Country { get; init; }

	/// <summary>
	/// Gets or initializes the image format the catalog knows, e.g. <c>PNG</c>.
	/// </summary>
	public string? Format { get; init; }

	/// <summary>
	/// Gets or initializes the width of the image in pixels.
	/// </summary>
	public float Width { get; init; }

	/// <summary>
	/// Gets or initializes the height of the image in pixels.
	/// </summary>
	public float Height { get; init; }

	/// <summary>
	/// Gets or initializes the keywords that describe this version of the logo, as one line.
	/// </summary>
	public string Tags { get; init; } = string.Empty;

	/// <summary>
	/// Gets or initializes the URL the logo is downloaded from, the remote path of an entry.
	/// </summary>
	public required string Url { get; init; }

	/// <summary>
	/// Gets or initializes the path of the downloaded file, <see langword="null"/> while the logo is
	/// not cached.
	/// </summary>
	public string? LocalPath { get; init; }

	/// <summary>
	/// Gets or initializes the size of the downloaded file in bytes.
	/// </summary>
	public long? FileSize { get; init; }

	/// <summary>
	/// Gets or initializes the moment the logo was downloaded.
	/// </summary>
	public DateTime? DownloadedAt { get; init; }

	/// <summary>
	/// Indicates whether the file of the logo is there, which is what a local path can be assigned
	/// from.
	/// </summary>
	/// <remarks>
	/// The database only knows that a logo was downloaded, so the service that reads a row asks the
	/// store and hands the answer to the mapping.
	/// </remarks>
	public bool IsCached { get; init; }
}
