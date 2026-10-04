// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

/// <summary>
/// Represents the clipboard of the window a view model is shown in.
/// </summary>
/// <remarks>
/// A path that is shown, like the URL or the cached file of a logo, is copied through here instead
/// of being retyped by hand.
/// </remarks>
public interface IClipboardService
{
	/// <summary>
	/// Puts a text onto the clipboard.
	/// </summary>
	/// <param name="text">The text to copy, nothing is copied for an empty one.</param>
	/// <returns><see langword="true"/> if the text was copied.</returns>
	Task<bool> SetTextAsync(string? text);
}
