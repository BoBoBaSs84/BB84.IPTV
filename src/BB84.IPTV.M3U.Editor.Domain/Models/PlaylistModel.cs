// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel.DataAnnotations;

using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Domain.Models;

/// <summary>
/// Represents an M3U playlist, which can contain multiple entries with associated metadata.
/// </summary>
public sealed class PlaylistModel : ValidatableObject, IPlaylist
{
	private string? _urlTvg;
	private int _cache;
	private Deinterlace _deinterlace;
	private int _refresh;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistModel"/> class.
	/// </summary>
	public PlaylistModel()
		=> Entries = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistModel"/> class.
	/// </summary>
	public PlaylistModel(IPlaylist playlist, IEnumerable<IEntry>? entries = null)
	{
		UrlTvg = playlist.UrlTvg;
		Cache = playlist.Cache;
		Deinterlace = playlist.Deinterlace;
		Refresh = playlist.Refresh;
		Entries = entries?.Select(entry => new EntryModel(entry)).ToList() ?? [];
	}

	/// <inheritdoc/>
	public string? UrlTvg
	{
		get => _urlTvg;
		set => SetProperty(ref _urlTvg, value);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Set to 0 if not specified.
	/// </remarks>
	[Range(0, int.MaxValue)]
	public int Cache
	{
		get => _cache;
		set => SetProperty(ref _cache, value);
	}

	/// <inheritdoc/>
	public Deinterlace Deinterlace
	{
		get => _deinterlace;
		set => SetProperty(ref _deinterlace, value);
	}

	/// <inheritdoc/>
	/// <remarks>
	/// Set to 0 if not specified.
	/// </remarks>
	[Range(0, int.MaxValue)]
	public int Refresh
	{
		get => _refresh;
		set => SetProperty(ref _refresh, value);
	}

	/// <inheritdoc/>
	public IEnumerable<EntryModel> Entries { get; }
}
