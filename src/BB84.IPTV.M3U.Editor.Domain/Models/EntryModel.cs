#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Domain.Models;

/// <summary>
/// Represents a media track entry in an M3U playlist.
/// </summary>
public sealed class EntryModel : ValidatableObject, IEntry
{
	private int _duration;
	private string _title;
	private string _filePath;
	private string? _grouping;

	/// <summary>
	/// Initializes a new instance of the <see cref="EntryModel"/> class.
	/// </summary>
	/// <param name="title">The title of the entry.</param>
	/// <param name="filePath">The file path or URL of the entry.</param>
	/// <param name="duration">The duration of the entry in seconds.</param>
	/// <param name="grouping">The grouping/category of the entry.</param>
	/// <param name="metadata">The metadata associated with the entry.</param>
	public EntryModel(string title, string filePath, int duration = -1, string? grouping = null, IMetadata? metadata = null)
	{
		Duration = duration;
		Title = title;
		FilePath = filePath;
		Grouping = grouping;
		Metadata = metadata ?? new MetadataModel();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EntryModel"/> class.
	/// </summary>
	/// <param name="entry">The entry to copy values from.</param>
	public EntryModel(IEntry entry)
	{
		Duration = entry.Duration;
		Title = entry.Title;
		FilePath = entry.FilePath;
		Grouping = entry.Grouping;
		Metadata = new MetadataModel(entry.Metadata);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The default value is -1, indicating an unknown duration.
	/// </remarks>
	public int Duration
	{
		get => _duration;
		set => SetProperty(ref _duration, value);
	}

	/// <inheritdoc/>
	public string Title
	{
		get => _title;
		set => SetProperty(ref _title, value);
	}

	/// <inheritdoc/>
	public string FilePath
	{
		get => _filePath;
		set => SetProperty(ref _filePath, value);
	}

	/// <inheritdoc/>
	public string? Grouping
	{
		get => _grouping;
		set => SetProperty(ref _grouping, value);
	}

	/// <inheritdoc/>
	public IMetadata Metadata { get; }
}
