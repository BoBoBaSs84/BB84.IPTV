// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Events.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Events;

/// <summary>
/// Represents the progress of a run of the logo cache.
/// </summary>
/// <param name="processedCount">The number of logos that are done.</param>
/// <param name="totalCount">The number of logos the run has to do.</param>
/// <param name="progressPercentage">The progress in percent.</param>
[GenerateToString]
public sealed partial class LogoCacheProgressEvent(int processedCount, int totalCount, int progressPercentage) : EventBase
{
	/// <summary>
	/// Gets the number of logos that are done.
	/// </summary>
	public int ProcessedCount => processedCount;

	/// <summary>
	/// Gets the number of logos the run has to do.
	/// </summary>
	public int TotalCount => totalCount;

	/// <summary>
	/// Gets the progress in percent.
	/// </summary>
	public int ProgressPercentage => progressPercentage;
}