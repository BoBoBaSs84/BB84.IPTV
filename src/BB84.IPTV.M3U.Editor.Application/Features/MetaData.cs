// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Features;

/// <summary>
/// Represents metadata information for a paged list, including current page, total pages,
/// page size, and total item count.
/// </summary>
/// <param name="currentPage">The current page.</param>
/// <param name="totalPages">The total pages.</param>
/// <param name="pageSize">The page size.</param>
/// <param name="totalCount">The total count.</param>
public sealed class MetaData(int currentPage, int totalPages, int pageSize, int totalCount)
{
	/// <summary>
	/// Gets the current page property.
	/// </summary>
	public int CurrentPage { get; } = currentPage;

	/// <summary>
	/// Gets the total pages property.
	/// </summary>
	public int TotalPages { get; } = totalPages;

	/// <summary>
	/// Gets the page size property.
	/// </summary>
	public int PageSize { get; } = pageSize;

	/// <summary>
	/// Gets the total count property.
	/// </summary>
	public int TotalCount { get; } = totalCount;

	/// <summary>
	/// Indicates whether there is a previous page available.
	/// </summary>
	public bool HasPrevious => CurrentPage > 1;

	/// <summary>
	/// Indicates whether there is a next page available.
	/// </summary>
	public bool HasNext => CurrentPage < TotalPages;
}