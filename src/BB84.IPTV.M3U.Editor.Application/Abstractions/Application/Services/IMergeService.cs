// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that merges stored playlists into a new one.
/// </summary>
/// <remarks>
/// The source playlists are only read, a merge never changes them.
/// </remarks>
public interface IMergeService
{
	/// <summary>
	/// Merges the playlists of the <paramref name="request"/> without storing the result.
	/// </summary>
	/// <param name="request">The playlists to merge and how they are merged.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The merged playlist and what happened while merging.</returns>
	Task<MergePreviewResponse> PreviewAsync(MergeRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Merges the playlists of the <paramref name="request"/> and stores the result as a new playlist.
	/// </summary>
	/// <param name="request">The playlists to merge and how they are merged.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The identifier of the new playlist.</returns>
	Task<int> MergeAsync(MergeRequest request, CancellationToken cancellationToken = default);
}