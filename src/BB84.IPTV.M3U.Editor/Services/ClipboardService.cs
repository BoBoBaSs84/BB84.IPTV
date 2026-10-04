// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using Avalonia.Input.Platform;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Extensions;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// The clipboard service class, backed by the clipboard of the active window.
/// </summary>
/// <remarks>
/// A window that is gone has no clipboard, which is reported as nothing copied instead of as a
/// failure: a copy is never worth a dialog.
/// </remarks>
[ExcludeFromCodeCoverage(Justification = "This class wraps the Avalonia clipboard which requires a UI thread.")]
internal sealed class ClipboardService : IClipboardService
{
	/// <inheritdoc/>
	public async Task<bool> SetTextAsync(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return false;

		IClipboard? clipboard = ApplicationExtensions.GetActiveWindow()?.Clipboard;

		if (clipboard is null)
			return false;

		await clipboard
			.SetTextAsync(text)
			.ConfigureAwait(true);

		return true;
	}
}
