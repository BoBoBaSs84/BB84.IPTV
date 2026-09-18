// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

/// <summary>
/// Represents metadata contract associated with an M3U entry.
/// </summary>
public interface IMetadata
{
	/// <summary>
	/// Indicates that the channel needs to be protected by parental control,
	/// if such is present in the device, can take values 0 and 1.
	/// </summary>
	bool Censored { get; set; }

	/// <summary>
	/// Get or set the unique identifier of the channel in the program file,
	/// if the XMLTV format is used for example.
	/// </summary>
	string? TvgId { get; set; }

	/// <summary>
	/// Get or set the name of the channel in the program file, which is used
	/// mainly when using programs in a format JTV.
	/// </summary>
	string? TvgName { get; set; }

	/// <summary>
	/// Get or set the TV guide logo URL or file path, which is used to display.
	/// </summary>
	string? TvgLogo { get; set; }

	/// <summary>
	/// Get or set the group identifier of the channels, which is used to group.
	/// </summary>
	string? GroupId { get; set; }

	/// <summary>
	/// Get or set the name or title of the group of channels, which is used to
	/// display.
	/// </summary>
	string? GroupTitle { get; set; }
}
