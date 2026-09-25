// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// Represents one entry of a playlist as it goes into a <c>channels.xml</c>.
/// </summary>
public sealed class GuideMappingResponse
{
	/// <summary>
	/// Gets or initializes what identifies the entry: its <c>tvg-id</c>, or its URL if it has none.
	/// </summary>
	public required string EntryKey { get; init; }

	/// <summary>
	/// Gets or initializes the title of the entry, shown so the row can be recognised.
	/// </summary>
	public required string Title { get; init; }

	/// <summary>
	/// Gets or initializes the site the guide is grabbed from, e.g. <c>example.com</c>.
	/// </summary>
	public string? Site { get; init; }

	/// <summary>
	/// Gets or initializes the identifier the channel has on the site.
	/// </summary>
	public string? SiteId { get; init; }

	/// <summary>
	/// Gets or initializes the language of the guide, e.g. <c>de</c>.
	/// </summary>
	public string? Lang { get; init; }

	/// <summary>
	/// Gets or initializes the identifier the guide is written under.
	/// </summary>
	public string? XmltvId { get; init; }

	/// <summary>
	/// Gets or initializes the name written into the <c>channels.xml</c>.
	/// </summary>
	public string? DisplayName { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org channel the entry was matched to, <see langword="null"/> if
	/// the entry holds nothing that names one.
	/// </summary>
	/// <remarks>
	/// What the guides of the entry are looked up with.
	/// </remarks>
	public string? Channel { get; init; }

	/// <summary>
	/// Gets or initializes the iptv-org feed the entry was created from, if any.
	/// </summary>
	public string? Feed { get; init; }

	/// <summary>
	/// Indicates whether the mapping was taken from a stored one instead of the imported guides.
	/// </summary>
	public bool IsStored { get; init; }

	/// <summary>
	/// Indicates whether the mapping holds everything a <c>channels.xml</c> entry needs.
	/// </summary>
	public bool IsComplete
		=> !string.IsNullOrWhiteSpace(Site)
		&& !string.IsNullOrWhiteSpace(SiteId)
		&& !string.IsNullOrWhiteSpace(XmltvId);
}
