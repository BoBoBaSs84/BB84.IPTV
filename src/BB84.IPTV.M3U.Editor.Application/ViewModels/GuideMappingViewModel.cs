// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents one entry of a playlist in the guide mapping table, editable in every column.
/// </summary>
public sealed class GuideMappingViewModel : ViewModelBase
{
	private string? _site;
	private string? _siteId;
	private string? _lang;
	private string? _xmltvId;
	private string? _displayName;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuideMappingViewModel"/> class.
	/// </summary>
	/// <param name="mapping">The mapping of the entry, stored or prefilled.</param>
	public GuideMappingViewModel(GuideMappingResponse mapping)
	{
		ArgumentNullException.ThrowIfNull(mapping);

		EntryKey = mapping.EntryKey;
		Title = mapping.Title;
		IsStored = mapping.IsStored;
		_site = mapping.Site;
		_siteId = mapping.SiteId;
		_lang = mapping.Lang;
		_xmltvId = mapping.XmltvId;
		_displayName = mapping.DisplayName;
		Channel = mapping.Channel;
		Feed = mapping.Feed;
	}

	/// <summary>
	/// Gets what identifies the entry: its <c>tvg-id</c>, or its URL if it has none.
	/// </summary>
	public string EntryKey { get; }

	/// <summary>
	/// Gets the iptv-org channel the entry was matched to, <see langword="null"/> if it names none.
	/// </summary>
	public string? Channel { get; }

	/// <summary>
	/// Gets the iptv-org feed the entry was created from, if any.
	/// </summary>
	public string? Feed { get; }

	/// <summary>
	/// Gets the title of the entry.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Indicates whether the mapping was taken from a stored one instead of the imported guides.
	/// </summary>
	public bool IsStored { get; }

	/// <summary>
	/// Gets or sets the site the guide is grabbed from, e.g. <c>example.com</c>.
	/// </summary>
	[NotifyChanged(nameof(IsComplete))]
	public string? Site
	{
		get => _site;
		set => SetProperty(ref _site, value);
	}

	/// <summary>
	/// Gets or sets the identifier the channel has on the site.
	/// </summary>
	[NotifyChanged(nameof(IsComplete))]
	public string? SiteId
	{
		get => _siteId;
		set => SetProperty(ref _siteId, value);
	}

	/// <summary>
	/// Gets or sets the language of the guide, e.g. <c>de</c>.
	/// </summary>
	public string? Lang
	{
		get => _lang;
		set => SetProperty(ref _lang, value);
	}

	/// <summary>
	/// Gets or sets the identifier the guide is written under.
	/// </summary>
	[NotifyChanged(nameof(IsComplete))]
	public string? XmltvId
	{
		get => _xmltvId;
		set => SetProperty(ref _xmltvId, value);
	}

	/// <summary>
	/// Gets or sets the name written into the <c>channels.xml</c>.
	/// </summary>
	public string? DisplayName
	{
		get => _displayName;
		set => SetProperty(ref _displayName, value);
	}

	/// <summary>
	/// Indicates whether the row holds everything a <c>channels.xml</c> entry needs.
	/// </summary>
	public bool IsComplete
		=> ToResponse().IsComplete;

	/// <summary>
	/// Takes the site, the site identifier and the language of a guide, the rest of the row stays
	/// as it is.
	/// </summary>
	/// <param name="option">The guide that was picked.</param>
	public void Apply(GuideOptionResponse option)
	{
		ArgumentNullException.ThrowIfNull(option);

		Site = option.Site;
		SiteId = option.SiteId;
		Lang = option.Lang;
	}

	/// <summary>
	/// Creates the mapping as it is edited.
	/// </summary>
	/// <returns>The mapping of the entry.</returns>
	public GuideMappingResponse ToResponse() => new()
	{
		EntryKey = EntryKey,
		Title = Title,
		Site = Site,
		SiteId = SiteId,
		Lang = Lang,
		XmltvId = XmltvId,
		DisplayName = DisplayName,
		Channel = Channel,
		Feed = Feed,
		IsStored = IsStored
	};
}
