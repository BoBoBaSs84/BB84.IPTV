// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents one value a catalog search can be filtered by.
/// </summary>
/// <param name="Code">The code stored on the catalog entities, e.g. <c>DE</c>.</param>
/// <param name="Name">The name shown to the user, e.g. <c>Germany</c>.</param>
public sealed record CatalogFilterValue(string Code, string Name)
{
	/// <summary>
	/// Returns the name, so the value can be shown as it is.
	/// </summary>
	/// <returns>The name of the value.</returns>
	public override string ToString()
		=> Name;
}
