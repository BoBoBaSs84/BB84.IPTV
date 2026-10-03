// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Builds the key that says whether an imported row and a row of the catalog are the same thing.
/// </summary>
/// <remarks>
/// Only four of the catalog tables have a unique index, so the synchronization matches the rows by
/// the fields iptv-org identifies them with. The parts are joined with a separator that cannot
/// appear in a value, and a part that is not set counts as empty, so a missing feed does not make
/// two rows differ.
/// </remarks>
internal static class CatalogKey
{
	/// <summary>
	/// The separator between two parts of a key, a character no identifier holds.
	/// </summary>
	private const char Separator = '\u001F';

	/// <summary>
	/// Builds the key of the given parts.
	/// </summary>
	/// <param name="parts">The values that identify the row.</param>
	/// <returns>The key, compared with <see cref="StringComparer.Ordinal"/>.</returns>
	internal static string Of(params string?[] parts)
		=> string.Join(Separator, parts.Select(part => part ?? string.Empty));
}
