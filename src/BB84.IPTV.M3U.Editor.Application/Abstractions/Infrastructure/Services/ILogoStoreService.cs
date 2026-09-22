// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents the store that holds the downloaded logo files.
/// </summary>
/// <remarks>
/// The files live below the per-user application data directory, one folder per iptv-org channel.
/// </remarks>
public interface ILogoStoreService
{
	/// <summary>
	/// Writes a downloaded logo to the store, replacing the file that is there.
	/// </summary>
	/// <param name="channel">The iptv-org channel identifier, it names the folder.</param>
	/// <param name="fileName">The name of the file, it is made safe for the file system.</param>
	/// <param name="content">The downloaded file.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The full path of the written file.</returns>
	Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default);

	/// <summary>
	/// Indicates whether the file at <paramref name="path"/> is there.
	/// </summary>
	/// <param name="path">The full path of the file.</param>
	/// <returns><see langword="true"/> if the file exists.</returns>
	bool Exists(string? path);

	/// <summary>
	/// Deletes every cached file.
	/// </summary>
	/// <returns>The number of deleted files.</returns>
	int Clear();
}