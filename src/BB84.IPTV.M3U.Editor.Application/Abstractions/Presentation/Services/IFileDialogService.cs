// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

/// <summary>
/// The interface for the file dialog service.
/// </summary>
public interface IFileDialogService
{
	/// <summary>
	/// Shows an open file dialog and returns the selected file path.
	/// </summary>
	/// <param name="filter">The file type filter string, e.g. <c>M3U Files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All Files (*.*)|*.*</c>.</param>
	/// <param name="title">The dialog title.</param>
	/// <returns>The selected file path, or <see langword="null"/> if cancelled.</returns>
	Task<string?> ShowOpenFileDialogAsync(string filter, string title);

	/// <summary>
	/// Shows a save file dialog and returns the selected file path.
	/// </summary>
	/// <param name="filter">The file type filter string, e.g. <c>M3U Files (*.m3u;*.m3u8)|*.m3u;*.m3u8|All Files (*.*)|*.*</c>.</param>
	/// <param name="title">The dialog title.</param>
	/// <param name="defaultFileName">The default file name.</param>
	/// <returns>The selected file path, or <see langword="null"/> if cancelled.</returns>
	Task<string?> ShowSaveFileDialogAsync(string filter, string title, string? defaultFileName = null);

	/// <summary>
	/// Shows a folder dialog and returns the selected folder path.
	/// </summary>
	/// <param name="title">The dialog title.</param>
	/// <param name="startPath">The folder the dialog opens in, if it is there.</param>
	/// <returns>The selected folder path, or <see langword="null"/> if cancelled.</returns>
	Task<string?> ShowOpenFolderDialogAsync(string title, string? startPath = null);
}