// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents the answer of a logo download.
/// </summary>
public sealed class LogoDownloadResponse
{
	/// <summary>
	/// Gets or initializes the downloaded file, empty if the server answered that nothing changed.
	/// </summary>
	public byte[] Content { get; init; } = [];

	/// <summary>
	/// Gets or initializes the entity tag of the file, <see langword="null"/> if the server sent none.
	/// </summary>
	public string? ETag { get; init; }

	/// <summary>
	/// Gets or initializes the media type the server reported, e.g. <c>image/png</c>.
	/// </summary>
	public string? ContentType { get; init; }

	/// <summary>
	/// Indicates whether the server answered that the cached file is still current.
	/// </summary>
	public bool NotModified { get; init; }
}