// Copyright: 2025 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using Avalonia.Platform.Storage;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Extensions;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// The file dialog service class, backed by the Avalonia storage provider.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class wraps Avalonia file pickers which require a UI thread.")]
internal sealed class FileDialogService : IFileDialogService
{
	/// <inheritdoc/>
	public async Task<string?> ShowOpenFileDialogAsync(string filter, string title)
	{
		FilePickerOpenOptions options = new()
		{
			Title = title,
			AllowMultiple = false,
			FileTypeFilter = ParseFilter(filter)
		};

		IReadOnlyList<IStorageFile> files = await GetStorageProvider()
			.OpenFilePickerAsync(options)
			.ConfigureAwait(true);

		return files.Count > 0 ? files[0].TryGetLocalPath() : null;
	}

	/// <inheritdoc/>
	public async Task<string?> ShowSaveFileDialogAsync(string filter, string title, string? defaultFileName = null)
	{
		FilePickerSaveOptions options = new()
		{
			Title = title,
			SuggestedFileName = defaultFileName,
			FileTypeChoices = ParseFilter(filter)
		};

		IStorageFile? file = await GetStorageProvider()
			.SaveFilePickerAsync(options)
			.ConfigureAwait(true);

		return file?.TryGetLocalPath();
	}

	private static IStorageProvider GetStorageProvider()
		=> ApplicationExtensions.GetActiveWindow()?.StorageProvider
			?? throw new InvalidOperationException("No window is available to host the file dialog.");

	/// <summary>
	/// Converts a filter string in the <c>Name|*.a;*.b|Name|*.*</c> format into Avalonia file types.
	/// </summary>
	/// <param name="filter">The filter string to convert.</param>
	/// <returns>The file types described by the filter string.</returns>
	internal static List<FilePickerFileType> ParseFilter(string filter)
	{
		string[] parts = filter.Split('|');
		List<FilePickerFileType> fileTypes = [];

		for (int i = 0; i + 1 < parts.Length; i += 2)
		{
			// "*.*" only matches names that contain a dot on Linux and macOS, "*" matches everything.
			List<string> patterns = [.. parts[i + 1]
				.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(pattern => pattern == "*.*" ? "*" : pattern)];

			fileTypes.Add(new FilePickerFileType(parts[i]) { Patterns = patterns });
		}

		return fileTypes;
	}
}