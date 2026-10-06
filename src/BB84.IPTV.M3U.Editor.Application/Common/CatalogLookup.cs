// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Reads what the catalog knows about channels that a guide or a logo names by its iptv-org
/// identifier.
/// </summary>
/// <remarks>
/// A guide and a logo hold no foreign key to their channel, because the catalog may be reset, so a
/// service reads its rows first and the channels of those rows in a second query.
/// </remarks>
internal static class CatalogLookup
{
	/// <summary>
	/// The number of channel identifiers per <c>IN</c> clause, so the parameter limit of SQLite is
	/// never reached.
	/// </summary>
	internal const int ChannelChunkSize = 500;

	/// <summary>
	/// Loads what the catalog knows about the given channels, in chunks, so the query stays within
	/// the parameter limit of SQLite.
	/// </summary>
	/// <param name="repositoryService">The repository service of the current scope.</param>
	/// <param name="channels">The iptv-org identifiers of the channels to load.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>The known channels by their identifier, the case of the identifier is ignored.</returns>
	internal static async Task<Dictionary<string, ChannelInfo>> LoadChannelsAsync(
		IRepositoryService repositoryService,
		IReadOnlyList<string> channels,
		CancellationToken cancellationToken)
	{
		Dictionary<string, ChannelInfo> channelsById = new(StringComparer.OrdinalIgnoreCase);

		foreach (string[] chunk in channels.Chunk(ChannelChunkSize))
		{
			IReadOnlyList<ChannelInfo> loaded = await repositoryService.Channels
				.GetListAsync(
					Mappings.ChannelToInfo,
					new Query<ChannelEntity> { Where = channel => chunk.Contains(channel.Channel) },
					cancellationToken)
				.ConfigureAwait(false);

			foreach (ChannelInfo info in loaded)
				channelsById[info.Channel] = info;
		}

		return channelsById;
	}

	/// <summary>
	/// Looks up what the catalog knows about a channel.
	/// </summary>
	/// <param name="channelsById">The channels loaded by <see cref="LoadChannelsAsync"/>.</param>
	/// <param name="channel">The iptv-org identifier, <see langword="null"/> if the row names none.</param>
	/// <returns>The channel, or <see langword="null"/> if the catalog does not know it.</returns>
	internal static ChannelInfo? Lookup(IReadOnlyDictionary<string, ChannelInfo> channelsById, string? channel)
		=> channel is not null && channelsById.TryGetValue(channel, out ChannelInfo? info) ? info : null;
}
