// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Represents the centralized mapping class for the application layer, providing methods
/// and properties to convert between different data models and entities.
/// </summary>
/// <remarks>
/// The mappings are split by the type they map, one partial per type. Projections the database
/// runs are exposed as static <see cref="Expression{TDelegate}"/> properties, so they are built
/// once and stay translatable to SQL.
/// </remarks>
internal static partial class Mappings
{
	/// <summary>
	/// Indicates whether the imported value differs from the stored one.
	/// </summary>
	/// <param name="stored">The value the catalog holds.</param>
	/// <param name="imported">The value iptv-org sent.</param>
	/// <returns><see langword="true"/> if the value changed.</returns>
	private static bool Differs(string? stored, string? imported)
		=> !string.Equals(stored, imported, StringComparison.Ordinal);

	/// <summary>
	/// Indicates whether the imported list differs from the stored one, in content and in order.
	/// </summary>
	/// <remarks>
	/// A list that is not set counts as empty, because the converter of the collection columns
	/// writes an empty list as <see langword="null"/> and reads it back as an empty one.
	/// </remarks>
	/// <param name="stored">The list the catalog holds.</param>
	/// <param name="imported">The list iptv-org sent.</param>
	/// <returns><see langword="true"/> if the list changed.</returns>
	private static bool Differs(ICollection<string>? stored, IReadOnlyList<string>? imported)
	{
		int storedCount = stored?.Count ?? 0;
		int importedCount = imported?.Count ?? 0;

		return storedCount != importedCount
			|| (storedCount > 0 && !stored!.SequenceEqual(imported!, StringComparer.Ordinal));
	}
}
