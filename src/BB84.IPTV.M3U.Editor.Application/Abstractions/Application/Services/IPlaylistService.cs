// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that stores playlists in the database and imports and exports them as M3U files.
/// </summary>
public interface IPlaylistService
{
	/// <summary>
	/// Gets a page of the stored playlists, ordered by name.
	/// </summary>
	/// <param name="request">The page to read, the first page with the default size if omitted.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The requested page of the summaries, with the number of stored playlists.</returns>
	Task<IPagedList<PlaylistSummaryResponse>> GetPlaylistsAsync(PlaylistSearchRequest? request = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Loads a stored playlist with its entries in their stored order.
	/// </summary>
	/// <param name="id">The identifier of the playlist.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The playlist, or <see langword="null"/> if no playlist with this identifier exists.</returns>
	Task<IPlaylist?> LoadAsync(int id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores a new playlist.
	/// </summary>
	/// <param name="name">The name of the playlist.</param>
	/// <param name="playlist">The playlist to store.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The identifier of the new playlist.</returns>
	Task<int> CreateAsync(string name, IPlaylist playlist, CancellationToken cancellationToken = default);

	/// <summary>
	/// Replaces the name, the header and all entries of a stored playlist in one commit.
	/// </summary>
	/// <param name="id">The identifier of the playlist.</param>
	/// <param name="name">The name of the playlist.</param>
	/// <param name="playlist">The new content of the playlist.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the playlist was updated; <see langword="false"/> if it does not exist.</returns>
	Task<bool> UpdateAsync(int id, string name, IPlaylist playlist, CancellationToken cancellationToken = default);

	/// <summary>
	/// Renames a stored playlist.
	/// </summary>
	/// <param name="id">The identifier of the playlist.</param>
	/// <param name="name">The new name.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the playlist was renamed; <see langword="false"/> if it does not exist.</returns>
	Task<bool> RenameAsync(int id, string name, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a stored playlist with all its entries.
	/// </summary>
	/// <param name="id">The identifier of the playlist.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the playlist was deleted; <see langword="false"/> if it does not exist.</returns>
	Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

	/// <summary>
	/// Reads an M3U file and stores it as a new playlist.
	/// </summary>
	/// <param name="filePath">The path of the M3U file.</param>
	/// <param name="name">The name of the playlist, the file name without extension if omitted.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The identifier of the new playlist.</returns>
	/// <exception cref="InvalidDataException">Thrown if the file is not a valid M3U playlist.</exception>
	Task<int> ImportAsync(string filePath, string? name = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes a stored playlist to an M3U file.
	/// </summary>
	/// <param name="id">The identifier of the playlist.</param>
	/// <param name="filePath">The path of the M3U file to write.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the playlist was exported; <see langword="false"/> if it does not exist.</returns>
	Task<bool> ExportAsync(int id, string filePath, CancellationToken cancellationToken = default);
}