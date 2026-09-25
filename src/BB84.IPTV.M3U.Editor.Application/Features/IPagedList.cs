// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Features;

/// <summary>
/// Represents a paged read-only list of items with associated metadata.
/// </summary>
/// <inheritdoc/>
public interface IPagedList<T> : IReadOnlyList<T>
{
	/// <summary>
	/// Gets the metadata associated with the paged list, such as total item
	/// count, page size, and current page index.
	/// </summary>
	MetaData MetaData { get; }
}