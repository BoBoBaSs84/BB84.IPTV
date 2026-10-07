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
	/// Converts a <see cref="StreamResponse"/> to a <see cref="StreamEntity"/>.
	/// </summary>
	/// <param name="response">The response to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static StreamEntity ToEntity(this StreamResponse response) => new()
	{
		Channel = response.Channel,
		Feed = response.Feed,
		Title = response.Title,
		Url = response.Url,
		Referrer = response.Referrer,
		UserAgent = response.UserAgent,
		Quality = response.Quality
	};

	/// <summary>
	/// Gets the key that identifies the stream of the import.
	/// </summary>
	/// <param name="response">The imported stream.</param>
	/// <returns>The key of the stream.</returns>
	internal static string GetKey(this StreamResponse response)
		=> CatalogKey.Of(response.Channel, response.Feed, response.Url);

	/// <summary>
	/// Gets the key that identifies the stored stream.
	/// </summary>
	/// <param name="entity">The stored stream.</param>
	/// <returns>The key of the stream.</returns>
	internal static string GetKey(this StreamEntity entity)
		=> CatalogKey.Of(entity.Channel, entity.Feed, entity.Url);

	/// <summary>
	/// Takes what the import holds into the stored stream.
	/// </summary>
	/// <param name="entity">The stored stream.</param>
	/// <param name="response">The imported stream.</param>
	/// <returns><see langword="true"/> if the stream changed.</returns>
	internal static bool Apply(this StreamEntity entity, StreamResponse response)
	{
		bool changed = false;

		if (Differs(entity.Title, response.Title))
		{
			entity.Title = response.Title;
			changed = true;
		}

		if (Differs(entity.Referrer, response.Referrer))
		{
			entity.Referrer = response.Referrer;
			changed = true;
		}

		if (Differs(entity.UserAgent, response.UserAgent))
		{
			entity.UserAgent = response.UserAgent;
			changed = true;
		}

		if (Differs(entity.Quality, response.Quality))
		{
			entity.Quality = response.Quality;
			changed = true;
		}

		return changed;
	}
}
