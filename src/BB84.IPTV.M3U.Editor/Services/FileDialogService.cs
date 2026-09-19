// Copyright: 2025 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

using Microsoft.Win32;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// The file dialog service class.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class wraps WPF file dialogs which require a UI thread.")]
internal sealed class FileDialogService : IFileDialogService
{
	/// <inheritdoc/>
	public string? ShowOpenFileDialog(string filter, string title)
	{
		OpenFileDialog dialog = new()
		{
			Filter = filter,
			Title = title
		};

		bool? result = dialog.ShowDialog();

		return result == true ? dialog.FileName : null;
	}

	/// <inheritdoc/>
	public string? ShowSaveFileDialog(string filter, string title, string? defaultFileName = null)
	{
		SaveFileDialog dialog = new()
		{
			Filter = filter,
			Title = title,
			FileName = defaultFileName ?? string.Empty
		};

		bool? result = dialog.ShowDialog();

		return result == true ? dialog.FileName : null;
	}
}
