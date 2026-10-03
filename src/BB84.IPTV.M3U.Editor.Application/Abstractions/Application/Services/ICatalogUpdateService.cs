// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that keeps the catalog from growing old without the user noticing.
/// </summary>
public interface ICatalogUpdateService
{
	/// <summary>
	/// Updates the catalog when it was read longer ago than the settings allow.
	/// </summary>
	/// <remarks>
	/// The user is asked first, unless the settings say to update on their own. A failure is
	/// reported through the event service instead of thrown, because nobody awaits the check.
	/// </remarks>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the catalog was updated.</returns>
	Task<bool> RunStartupCheckAsync(CancellationToken cancellationToken = default);
}
