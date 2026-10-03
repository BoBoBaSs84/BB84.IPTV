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
	/// Converts a <see cref="StreamRequest"/> to a <see cref="StreamEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static StreamEntity ToEntity(this StreamRequest request) => new()
	{
		Channel = request.Channel,
		Feed = request.Feed,
		Title = request.Title,
		Url = request.Url,
		Referrer = request.Referrer,
		UserAgent = request.UserAgent,
		Quality = request.Quality
	};

	/// <summary>
	/// Gets the key that identifies the stream of the import.
	/// </summary>
	/// <param name="request">The imported stream.</param>
	/// <returns>The key of the stream.</returns>
	internal static string GetKey(this StreamRequest request)
		=> CatalogKey.Of(request.Channel, request.Feed, request.Url);

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
	/// <param name="request">The imported stream.</param>
	/// <returns><see langword="true"/> if the stream changed.</returns>
	internal static bool Apply(this StreamEntity entity, StreamRequest request)
	{
		bool changed = false;

		if (Differs(entity.Title, request.Title))
		{
			entity.Title = request.Title;
			changed = true;
		}

		if (Differs(entity.Referrer, request.Referrer))
		{
			entity.Referrer = request.Referrer;
			changed = true;
		}

		if (Differs(entity.UserAgent, request.UserAgent))
		{
			entity.UserAgent = request.UserAgent;
			changed = true;
		}

		if (Differs(entity.Quality, request.Quality))
		{
			entity.Quality = request.Quality;
			changed = true;
		}

		return changed;
	}
}
