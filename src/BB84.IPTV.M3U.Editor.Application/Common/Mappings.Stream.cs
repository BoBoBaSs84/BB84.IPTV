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
}
