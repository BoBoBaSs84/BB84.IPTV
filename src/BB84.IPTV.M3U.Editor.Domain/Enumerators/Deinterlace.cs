// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Domain.Enumerators;

/// <summary>
/// Represents deinterlace methods for video streams.
/// </summary>
public enum Deinterlace
{
	/// <summary>
	/// Represents the absence of any options or a default state.
	/// </summary>
	None = 0,
	
	/// <summary>
	/// Specifies a blending mode for combining colors.
	/// </summary>
	Blend = 1,
	
	/// <summary>
	/// Represents the arithmetic mean aggregation type.
	/// </summary>
	/// <remarks>
	/// This value is typically used to specify that the mean (average)
	/// should be calculated when performing an aggregation operation.
	/// </remarks>
	Mean = 2
}
