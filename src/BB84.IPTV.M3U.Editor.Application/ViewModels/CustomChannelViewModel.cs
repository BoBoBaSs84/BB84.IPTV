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
/// Represents a user defined channel in the custom channel list, editable in the detail panel.
/// </summary>
public sealed class CustomChannelViewModel : ViewModelBase
{
	private string _name;
	private string _url;
	private string? _groupTitle;
	private string? _tvgId;
	private string? _tvgLogo;

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomChannelViewModel"/> class.
	/// </summary>
	/// <param name="channel">The stored custom channel.</param>
	public CustomChannelViewModel(CustomChannelResponse channel)
	{
		ArgumentNullException.ThrowIfNull(channel);

		Id = channel.Id;
		_name = channel.Name;
		_url = channel.Url;
		_groupTitle = channel.GroupTitle;
		_tvgId = channel.TvgId;
		_tvgLogo = channel.TvgLogo;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomChannelViewModel"/> class, for a channel
	/// that has not been stored yet.
	/// </summary>
	/// <param name="name">The suggested name of the channel.</param>
	public CustomChannelViewModel(string name)
	{
		Id = 0;
		_name = name;
		_url = string.Empty;
	}

	/// <summary>
	/// Gets the identifier of the stored custom channel, <c>0</c> as long as it is not stored.
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Indicates whether the channel has not been stored yet.
	/// </summary>
	public bool IsNew
		=> Id is 0;

	/// <summary>
	/// Gets or sets the name of the channel.
	/// </summary>
	[NotifyChanged(nameof(IsValid))]
	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}

	/// <summary>
	/// Gets or sets the link to the media source, any URL scheme is allowed.
	/// </summary>
	[NotifyChanged(nameof(IsValid))]
	public string Url
	{
		get => _url;
		set => SetProperty(ref _url, value);
	}

	/// <summary>
	/// Gets or sets the group title (<c>group-title</c>).
	/// </summary>
	public string? GroupTitle
	{
		get => _groupTitle;
		set => SetProperty(ref _groupTitle, value);
	}

	/// <summary>
	/// Gets or sets the TV guide identifier (<c>tvg-id</c>).
	/// </summary>
	public string? TvgId
	{
		get => _tvgId;
		set => SetProperty(ref _tvgId, value);
	}

	/// <summary>
	/// Gets or sets the logo URL or file path (<c>tvg-logo</c>).
	/// </summary>
	public string? TvgLogo
	{
		get => _tvgLogo;
		set => SetProperty(ref _tvgLogo, value);
	}

	/// <summary>
	/// Indicates whether the channel can be stored as it is.
	/// </summary>
	public bool IsValid
		=> !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Url);

	/// <summary>
	/// Creates the stored representation of the channel.
	/// </summary>
	/// <returns>The custom channel as it is edited.</returns>
	public CustomChannelResponse ToResponse() => new()
	{
		Id = Id,
		Name = Name.Trim(),
		Url = Url.Trim(),
		GroupTitle = GroupTitle,
		TvgId = TvgId,
		TvgLogo = TvgLogo
	};

	/// <summary>
	/// Takes over the identifier the channel got when it was stored the first time.
	/// </summary>
	/// <param name="id">The identifier of the stored custom channel.</param>
	public void MarkStored(int id)
	{
		Id = id;
		RaisePropertyChanged(nameof(Id));
		RaisePropertyChanged(nameof(IsNew));
	}
}
