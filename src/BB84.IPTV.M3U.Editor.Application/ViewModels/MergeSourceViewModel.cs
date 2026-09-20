// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents a stored playlist that can take part in a merge.
/// </summary>
public sealed class MergeSourceViewModel : ViewModelBase
{
	private bool _isSelected;

	/// <summary>
	/// Initializes a new instance of the <see cref="MergeSourceViewModel"/> class.
	/// </summary>
	/// <param name="summary">The summary of the stored playlist.</param>
	public MergeSourceViewModel(PlaylistSummaryResponse summary)
	{
		ArgumentNullException.ThrowIfNull(summary);

		Id = summary.Id;
		Name = summary.Name;
		EntryCount = summary.EntryCount;
	}

	/// <summary>
	/// Gets the identifier of the stored playlist.
	/// </summary>
	public int Id { get; }

	/// <summary>
	/// Gets the name of the playlist.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the number of entries in the playlist.
	/// </summary>
	public int EntryCount { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the playlist takes part in the merge.
	/// </summary>
	public bool IsSelected
	{
		get => _isSelected;
		set => SetProperty(ref _isSelected, value);
	}
}