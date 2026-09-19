// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Domain.Models;

/// <summary>
/// Represents metadata associated with an M3U entry.
/// </summary>
public sealed class MetadataModel : ValidatableObject, IMetadata
{
	private bool _censored;
	private string? _tvgId;
	private string? _tvgName;
	private string? _tvgLogo;
	private string? _groupId;
	private string? _groupTitle;
	private string? _additionalAttributes;

	/// <summary>
	/// Initializes a new instance of the <see cref="MetadataModel"/> class.
	/// </summary>
	public MetadataModel()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="MetadataModel"/> class.
	/// </summary>
	/// <param name="metadata">The metadata to copy values from.</param>
	public MetadataModel(IMetadata metadata)
	{
		Censored = metadata.Censored;
		TvgId = metadata.TvgId;
		TvgName = metadata.TvgName;
		TvgLogo = metadata.TvgLogo;
		GroupId = metadata.GroupId;
		GroupTitle = metadata.GroupTitle;
		AdditionalAttributes = metadata.AdditionalAttributes;
	}

	/// <inheritdoc/>
	public bool Censored
	{
		get => _censored;
		set => SetProperty(ref _censored, value);
	}

	/// <inheritdoc/>
	public string? TvgId
	{
		get => _tvgId;
		set => SetProperty(ref _tvgId, value);
	}

	/// <inheritdoc/>
	public string? TvgName
	{
		get => _tvgName;
		set => SetProperty(ref _tvgName, value);
	}

	/// <inheritdoc/>
	public string? TvgLogo
	{
		get => _tvgLogo;
		set => SetProperty(ref _tvgLogo, value);
	}

	/// <inheritdoc/>
	public string? GroupId
	{
		get => _groupId;
		set => SetProperty(ref _groupId, value);
	}

	/// <inheritdoc/>
	public string? GroupTitle
	{
		get => _groupTitle;
		set => SetProperty(ref _groupTitle, value);
	}

	/// <inheritdoc/>
	public string? AdditionalAttributes
	{
		get => _additionalAttributes;
		set => SetProperty(ref _additionalAttributes, value);
	}
}
