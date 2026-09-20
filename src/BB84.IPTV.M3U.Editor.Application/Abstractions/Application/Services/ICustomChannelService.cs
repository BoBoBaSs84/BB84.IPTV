// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the service that stores the user defined channels.
/// </summary>
public interface ICustomChannelService
{
	/// <summary>
	/// Gets a page of the stored custom channels, ordered by name.
	/// </summary>
	/// <param name="request">The page to read, the first page with the default size if omitted.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The requested page of the custom channels, with the number of stored ones.</returns>
	Task<IPagedList<CustomChannelResponse>> GetChannelsAsync(CustomChannelSearchRequest? request = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores a new custom channel.
	/// </summary>
	/// <param name="channel">The channel to store, its identifier is ignored.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns>The identifier of the new custom channel.</returns>
	Task<int> CreateAsync(CustomChannelResponse channel, CancellationToken cancellationToken = default);

	/// <summary>
	/// Replaces a stored custom channel.
	/// </summary>
	/// <param name="channel">The channel to store, identified by its identifier.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the channel was updated; <see langword="false"/> if it does not exist.</returns>
	Task<bool> UpdateAsync(CustomChannelResponse channel, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes a stored custom channel.
	/// </summary>
	/// <param name="id">The identifier of the custom channel.</param>
	/// <param name="cancellationToken">The cancellation token, for cancelling the operation if needed.</param>
	/// <returns><see langword="true"/> if the channel was deleted; <see langword="false"/> if it does not exist.</returns>
	Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}