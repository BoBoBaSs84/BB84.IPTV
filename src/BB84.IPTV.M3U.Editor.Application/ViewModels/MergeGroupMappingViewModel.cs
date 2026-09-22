// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents one group title of a merge preview and the title it is renamed to.
/// </summary>
public sealed class MergeGroupMappingViewModel : ViewModelBase
{
	private string _targetGroup;

	/// <summary>
	/// Initializes a new instance of the <see cref="MergeGroupMappingViewModel"/> class.
	/// </summary>
	/// <param name="sourceGroup">The group title as it is in the sources, empty if the entries have none.</param>
	public MergeGroupMappingViewModel(string sourceGroup)
	{
		SourceGroup = sourceGroup;
		_targetGroup = sourceGroup;
	}

	/// <summary>
	/// Gets the group title as it is in the sources, an empty string for entries without one.
	/// </summary>
	public string SourceGroup { get; }

	/// <summary>
	/// Gets or sets the group title the entries get, an empty value clears the group title.
	/// </summary>
	public string TargetGroup
	{
		get => _targetGroup;
		set => SetProperty(ref _targetGroup, value);
	}

	/// <summary>
	/// Indicates whether the group title is renamed.
	/// </summary>
	public bool IsMapped
		=> !string.Equals(SourceGroup, TargetGroup, StringComparison.Ordinal);
}