// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents how the path of a cached logo is written into an exported playlist.
/// </summary>
public enum LogoPathStyle
{
	/// <summary>
	/// The full path of the cached file, which works wherever the playlist is opened on this machine.
	/// </summary>
	Absolute = 0,

	/// <summary>
	/// The path from the exported file to the cached file, which survives moving both together.
	/// </summary>
	Relative = 1
}