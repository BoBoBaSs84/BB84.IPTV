// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Common;

internal static partial class Mappings
{
	/// <summary>
	/// Converts a <see cref="FeedRequest"/> to a <see cref="FeedEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static FeedEntity ToEntity(this FeedRequest request) => new()
	{
		Channel = request.Channel,
		Feed = request.Id,
		Name = request.Name,
		AltNames = [.. request.AltNames],
		IsMain = request.IsMain,
		BroadcastArea = [.. request.BroadcastArea],
		Timezones = [.. request.Timezones],
		Languages = [.. request.Languages],
		Format = request.Format
	};

	/// <summary>
	/// Gets the key that identifies the feed of the import.
	/// </summary>
	/// <param name="request">The imported feed.</param>
	/// <returns>The key of the feed.</returns>
	internal static string GetKey(this FeedRequest request)
		=> CatalogKey.Of(request.Channel, request.Id);

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
	/// <param name="request">The imported feed.</param>
	/// <returns><see langword="true"/> if the feed changed.</returns>
	internal static bool Apply(this FeedEntity entity, FeedRequest request)
	{
		bool changed = false;

		if (Differs(entity.Name, request.Name))
		{
			entity.Name = request.Name;
			changed = true;
		}

		if (entity.IsMain != request.IsMain)
		{
			entity.IsMain = request.IsMain;
			changed = true;
		}

		if (Differs(entity.Format, request.Format))
		{
			entity.Format = request.Format;
			changed = true;
		}

		if (Differs(entity.AltNames, request.AltNames))
		{
			entity.AltNames = [.. request.AltNames];
			changed = true;
		}

		if (Differs(entity.BroadcastArea, request.BroadcastArea))
		{
			entity.BroadcastArea = [.. request.BroadcastArea];
			changed = true;
		}

		if (Differs(entity.Timezones, request.Timezones))
		{
			entity.Timezones = [.. request.Timezones];
			changed = true;
		}

		if (Differs(entity.Languages, request.Languages))
		{
			entity.Languages = [.. request.Languages];
			changed = true;
		}

		return changed;
	}
}
