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
}
