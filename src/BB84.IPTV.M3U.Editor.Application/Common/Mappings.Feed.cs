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
}
