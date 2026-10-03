// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is published when one list of the catalog was synchronized.
/// </summary>
/// <param name="kind">The list of the catalog that was synchronized.</param>
/// <param name="added">The number of rows that were added.</param>
/// <param name="updated">The number of rows that were updated.</param>
/// <param name="removed">The number of rows that were removed.</param>
/// <param name="completedTasks">The number of lists that are done.</param>
/// <param name="totalTasks">The total number of lists.</param>
[GenerateToString]
public sealed partial class CatalogSyncProgressEvent(CatalogKind kind, int added, int updated, int removed, int completedTasks, int totalTasks) : EventBase
{
	/// <summary>
	/// Gets the list of the catalog that was synchronized.
	/// </summary>
	public CatalogKind Kind { get; } = kind;

	/// <summary>
	/// Gets the number of rows that were added.
	/// </summary>
	public int Added { get; } = added;

	/// <summary>
	/// Gets the number of rows that were updated.
	/// </summary>
	public int Updated { get; } = updated;

	/// <summary>
	/// Gets the number of rows that were removed.
	/// </summary>
	public int Removed { get; } = removed;

	/// <summary>
	/// Gets the number of lists that are done.
	/// </summary>
	public int CompletedTasks { get; } = completedTasks;

	/// <summary>
	/// Gets the total number of lists.
	/// </summary>
	public int TotalTasks { get; } = totalTasks;

	/// <summary>
	/// Gets the progress percentage (0-100).
	/// </summary>
	public int ProgressPercentage => TotalTasks > 0 ? CompletedTasks * 100 / TotalTasks : 0;
}
