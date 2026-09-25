// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents a file service interface for loading and saving playlists.
/// </summary>
public interface IFileService
{
	/// <summary>
	/// Asynchronously loads a playlist from the specified file path.
	/// </summary>
	/// <param name="filePath">The path to the playlist file to load.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the load operation.</param>
	/// <returns>A task that represents the asynchronous load operation. The task result contains the loaded
	/// <see cref="IPlaylist"/> instance.</returns>
	Task<IPlaylist?> LoadAsync(string filePath, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously saves the specified playlist to a file at the given path.
	/// </summary>
	/// <param name="playlist">The playlist to save. Cannot be null.</param>
	/// <param name="filePath">The full file path where the playlist will be saved. Cannot be null or empty.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
	/// <returns>A task that represents the asynchronous save operation.</returns>
	Task Save(IPlaylist playlist, string filePath, CancellationToken cancellationToken = default);
}
