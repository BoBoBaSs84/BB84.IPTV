// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Converts a <see cref="FeedResponse"/> to a <see cref="FeedEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static FeedEntity ToEntity(this FeedResponse response) => new()
	{
		Channel = response.Channel,
		Feed = response.Id,
		Name = response.Name,
		AltNames = [.. response.AltNames],
		IsMain = response.IsMain,
		BroadcastArea = [.. response.BroadcastArea],
		Timezones = [.. response.Timezones],
		Languages = [.. response.Languages],
		Format = response.Format
	};

	/// <summary>
	/// Gets the key that identifies the feed of the import.
	/// </summary>
	/// <param name="response">The imported feed.</param>
	/// <returns>The key of the feed.</returns>
	internal static string GetKey(this FeedResponse response)
		=> CatalogKey.Of(response.Channel, response.Id);

	/// <summary>
	/// Gets the key that identifies the stored feed.
	/// </summary>
	/// <param name="entity">The stored feed.</param>
	/// <returns>The key of the feed.</returns>
	internal static string GetKey(this FeedEntity entity)
		=> CatalogKey.Of(entity.Channel, entity.Feed);

	/// <summary>
	/// Takes what the import holds into the stored feed.
	/// </summary>
	/// <param name="entity">The stored feed.</param>
	/// <param name="response">The imported feed.</param>
	/// <returns><see langword="true"/> if the feed changed.</returns>
	internal static bool Apply(this FeedEntity entity, FeedResponse response)
	{
		bool changed = false;

		if (Differs(entity.Name, response.Name))
		{
			entity.Name = response.Name;
			changed = true;
		}

		if (entity.IsMain != response.IsMain)
		{
			entity.IsMain = response.IsMain;
			changed = true;
		}

		if (Differs(entity.Format, response.Format))
		{
			entity.Format = response.Format;
			changed = true;
		}

		if (Differs(entity.AltNames, response.AltNames))
		{
			entity.AltNames = [.. response.AltNames];
			changed = true;
		}

		if (Differs(entity.BroadcastArea, response.BroadcastArea))
		{
			entity.BroadcastArea = [.. response.BroadcastArea];
			changed = true;
		}

		if (Differs(entity.Timezones, response.Timezones))
		{
			entity.Timezones = [.. response.Timezones];
			changed = true;
		}

		if (Differs(entity.Languages, response.Languages))
		{
			entity.Languages = [.. response.Languages];
			changed = true;
		}

		return changed;
	}
}
