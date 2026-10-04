// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents one entry of a playlist in the logo table, with the logo it carries.
/// </summary>
/// <remarks>
/// The row writes into the entry it was created from, so the playlist that is saved afterwards
/// holds what was assigned here.
/// </remarks>
public sealed class LogoAssignmentViewModel : ViewModelBase
{
	/// <summary>
	/// The character that separates the channel from the feed in a <c>tvg-id</c>.
	/// </summary>
	private const char FeedSeparator = '@';

	private readonly IEntry _entry;
	private string? _localPath;

	/// <summary>
	/// Initializes a new instance of the <see cref="LogoAssignmentViewModel"/> class.
	/// </summary>
	/// <param name="entry">The entry of the playlist the row belongs to.</param>
	/// <exception cref="ArgumentNullException">Thrown when the entry is <see langword="null"/>.</exception>
	public LogoAssignmentViewModel(IEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry);

		_entry = entry;
	}

	/// <summary>
	/// Gets the title of the entry.
	/// </summary>
	public string Title
		=> _entry.Title;

	/// <summary>
	/// Gets the <c>tvg-id</c> of the entry, <see langword="null"/> if it has none.
	/// </summary>
	public string? TvgId
		=> _entry.Metadata.TvgId;

	/// <summary>
	/// Gets the iptv-org channel of the entry, read from the entry itself or from its <c>tvg-id</c>,
	/// <see langword="null"/> if neither names one.
	/// </summary>
	/// <remarks>
	/// An entry that was created from the catalog carries the channel, an imported one only carries
	/// the <c>tvg-id</c> it was written with, which is the channel and, after an <c>@</c>, the feed.
	/// </remarks>
	public string? Channel
		=> _entry.Channel ?? ChannelOfTvgId();

	/// <summary>
	/// Gets the iptv-org feed of the entry, <see langword="null"/> if it names none.
	/// </summary>
	public string? Feed
		=> _entry.Feed ?? FeedOfTvgId();

	/// <summary>
	/// Gets or sets the <c>tvg-logo</c> of the entry, a URL or the path of a file.
	/// </summary>
	public string? Logo
	{
		get => _entry.Metadata.TvgLogo;
		set
		{
			if (string.Equals(_entry.Metadata.TvgLogo, value, StringComparison.Ordinal))
				return;

			_entry.Metadata.TvgLogo = value;
			RaisePropertyChanged(nameof(Logo));
			RaisePropertyChanged(nameof(HasLogo));
		}
	}

	/// <summary>
	/// Gets the cached file the logo of the entry stands for, <see langword="null"/> while the logo
	/// is not cached or is already a path.
	/// </summary>
	public string? LocalPath
	{
		get => _localPath;
		internal set => SetProperty(ref _localPath, value);
	}

	/// <summary>
	/// Indicates whether the entry carries a logo at all.
	/// </summary>
	public bool HasLogo
		=> !string.IsNullOrWhiteSpace(Logo);

	/// <summary>
	/// Reads the channel out of the <c>tvg-id</c>, which is the part before the <c>@</c>.
	/// </summary>
	private string? ChannelOfTvgId()
	{
		if (string.IsNullOrWhiteSpace(TvgId))
			return null;

		int separator = TvgId.IndexOf(FeedSeparator, StringComparison.Ordinal);

		return separator > 0 ? TvgId[..separator] : TvgId;
	}

	/// <summary>
	/// Reads the feed out of the <c>tvg-id</c>, which is the part after the <c>@</c>.
	/// </summary>
	private string? FeedOfTvgId()
	{
		if (string.IsNullOrWhiteSpace(TvgId))
			return null;

		int separator = TvgId.IndexOf(FeedSeparator, StringComparison.Ordinal);

		return separator > 0 && separator < TvgId.Length - 1 ? TvgId[(separator + 1)..] : null;
	}
}
