// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents the service that downloads a file from any host, e.g. a channel logo.
/// </summary>
public interface IDownloadService
{
	/// <summary>
	/// Downloads the file at <paramref name="url"/>.
	/// </summary>
	/// <remarks>
	/// With an <paramref name="eTag"/> the server is asked for the file only if it changed, so a
	/// repeated run does not download what is already cached.
	/// </remarks>
	/// <param name="url">The address of the file.</param>
	/// <param name="eTag">The entity tag of the cached file, if there is one.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The downloaded file, or <see langword="null"/> if the file could not be downloaded.</returns>
	Task<LogoDownloadResponse?> DownloadAsync(string url, string? eTag = null, CancellationToken cancellationToken = default);
}