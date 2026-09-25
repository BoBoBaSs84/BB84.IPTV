// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents an event that is triggered when a database import operation progresses.
/// </summary>
/// <param name="repositoryName">The name of the repository that was imported.</param>
/// <param name="recordsImported">The number of records that were imported.</param>
/// <param name="completedTasks">The number of tasks that have been completed.</param>
/// <param name="totalTasks">The total number of import tasks.</param>
[GenerateToString]
public sealed partial class DatabaseImportProgressEvent(string repositoryName, int recordsImported, int completedTasks, int totalTasks) : EventBase
{
	/// <summary>
	/// Gets the name of the repository that was imported.
	/// </summary>
	public string RepositoryName { get; } = repositoryName;

	/// <summary>
	/// Gets the number of records that were imported.
	/// </summary>
	public int RecordsImported { get; } = recordsImported;

	/// <summary>
	/// Gets the number of tasks that have been completed.
	/// </summary>
	public int CompletedTasks { get; } = completedTasks;

	/// <summary>
	/// Gets the total number of import tasks.
	/// </summary>
	public int TotalTasks { get; } = totalTasks;

	/// <summary>
	/// Gets the progress percentage (0-100).
	/// </summary>
	public int ProgressPercentage => TotalTasks > 0 ? CompletedTasks * 100 / TotalTasks : 0;
}
