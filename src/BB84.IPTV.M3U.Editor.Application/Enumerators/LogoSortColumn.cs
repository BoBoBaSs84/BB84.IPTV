// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents the column a logo search is ordered by.
/// </summary>
/// <remarks>
/// The names are the property names of
/// <see cref="Contracts.Responses.LogoOptionResponse"/>, so a grid column can be mapped by its
/// sort member path. Only columns of the logo itself are named: the name and the country of its
/// channel come from a second read and are therefore shown, but neither searched by order nor
/// ordered by.
/// </remarks>
public enum LogoSortColumn
{
	/// <summary>
	/// Ordered by the iptv-org channel identifier.
	/// </summary>
	Channel = 0,

	/// <summary>
	/// Ordered by the iptv-org feed identifier.
	/// </summary>
	Feed = 1,

	/// <summary>
	/// Ordered by the image format the catalog knows.
	/// </summary>
	Format = 2,

	/// <summary>
	/// Ordered by the width of the image.
	/// </summary>
	Width = 3,

	/// <summary>
	/// Ordered by the height of the image.
	/// </summary>
	Height = 4,

	/// <summary>
	/// Ordered by the URL the logo is downloaded from.
	/// </summary>
	Url = 5,

	/// <summary>
	/// Ordered by the path of the downloaded file.
	/// </summary>
	LocalPath = 6,

	/// <summary>
	/// Ordered by the size of the downloaded file.
	/// </summary>
	FileSize = 7,

	/// <summary>
	/// Ordered by the moment the logo was downloaded.
	/// </summary>
	DownloadedAt = 8
}
