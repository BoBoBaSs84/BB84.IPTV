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
	/// Converts what is stored about a synchronized list to its response.
	/// </summary>
	/// <param name="entity">The stored state of the list.</param>
	/// <returns>The converted response.</returns>
	internal static CatalogStatusResponse ToResponse(this CatalogSyncEntity entity) => new()
	{
		Kind = entity.Kind,
		FirstImported = entity.FirstImported,
		LastChecked = entity.LastChecked,
		LastChanged = entity.LastChanged,
		Added = entity.Added,
		Updated = entity.Updated,
		Removed = entity.Removed
	};
}
