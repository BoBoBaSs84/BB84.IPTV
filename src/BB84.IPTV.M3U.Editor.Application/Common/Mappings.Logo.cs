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
	/// Converts a <see cref="LogoRequest"/> to a <see cref="LogoEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	internal static LogoEntity ToEntity(this LogoRequest request) => new()
	{
		Channel = request.Channel,
		Feed = request.Feed,
		Format = request.Format,
		Height = request.Height,
		Width = request.Width,
		Tags = [.. request.Tags],
		Url = request.Url
	};

	/// <summary>
	/// Gets the key that identifies the logo of the import.
	/// </summary>
	/// <param name="request">The imported logo.</param>
	/// <returns>The key of the logo.</returns>
	internal static string GetKey(this LogoRequest request)
		=> CatalogKey.Of(request.Channel, request.Feed, request.Url);

	/// <summary>
	/// Gets the key that identifies the stored logo.
	/// </summary>
	/// <param name="entity">The stored logo.</param>
	/// <returns>The key of the logo.</returns>
	internal static string GetKey(this LogoEntity entity)
		=> CatalogKey.Of(entity.Channel, entity.Feed, entity.Url);

	/// <summary>
	/// Takes what the import holds into the stored logo.
	/// </summary>
	/// <remarks>
	/// The columns of the logo cache are left alone, so a logo that is already downloaded stays
	/// cached when the catalog changes its size or its tags.
	/// </remarks>
	/// <param name="entity">The stored logo.</param>
	/// <param name="request">The imported logo.</param>
	/// <returns><see langword="true"/> if the logo changed.</returns>
	internal static bool Apply(this LogoEntity entity, LogoRequest request)
	{
		bool changed = false;

		if (Differs(entity.Format, request.Format))
		{
			entity.Format = request.Format;
			changed = true;
		}

		if (entity.Width != request.Width)
		{
			entity.Width = request.Width;
			changed = true;
		}

		if (entity.Height != request.Height)
		{
			entity.Height = request.Height;
			changed = true;
		}

		if (Differs(entity.Tags, request.Tags))
		{
			entity.Tags = [.. request.Tags];
			changed = true;
		}

		return changed;
	}
}
