// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
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
	/// The separator the tags of a logo are shown with.
	/// </summary>
	private const string TagSeparator = ", ";

	/// <summary>
	/// Creates the option a logo is picked by, with what the catalog knows about its channel.
	/// </summary>
	/// <remarks>
	/// An empty tag collection is stored as <see langword="null"/>, and the value converter is not
	/// used for <see langword="null"/>, so the tags read from the database can be
	/// <see langword="null"/>.
	/// </remarks>
	/// <param name="entity">The stored logo to map.</param>
	/// <param name="channel">What the catalog knows about the channel of the logo, if anything.</param>
	/// <param name="isCached">Whether the file of the logo is in the store.</param>
	/// <returns>The logo option.</returns>
	internal static LogoOptionResponse ToOption(this LogoEntity entity, ChannelInfo? channel = null, bool isCached = false) => new()
	{
		Id = entity.Id,
		Channel = entity.Channel,
		Feed = entity.Feed,
		ChannelName = channel?.Name,
		Country = channel?.Country,
		Format = entity.Format,
		Width = entity.Width,
		Height = entity.Height,
		Tags = string.Join(TagSeparator, entity.Tags ?? []),
		Url = entity.Url,
		LocalPath = entity.LocalPath,
		FileSize = entity.FileSize,
		DownloadedAt = entity.DownloadedAt,
		IsCached = isCached
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
