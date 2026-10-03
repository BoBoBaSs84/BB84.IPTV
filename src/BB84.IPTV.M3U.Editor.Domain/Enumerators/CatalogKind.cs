// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Domain.Enumerators;

/// <summary>
/// Represents one of the lists the iptv-org catalog is imported from.
/// </summary>
/// <remarks>
/// The value is stored, so a member keeps its name once it is released.
/// </remarks>
public enum CatalogKind
{
	/// <summary>
	/// The categories a channel can belong to.
	/// </summary>
	Category = 1,

	/// <summary>
	/// The countries a channel can be broadcast in.
	/// </summary>
	Country = 2,

	/// <summary>
	/// The languages a channel can be broadcast in.
	/// </summary>
	Language = 3,

	/// <summary>
	/// The channels of the catalog.
	/// </summary>
	Channel = 4,

	/// <summary>
	/// The feeds a channel is broadcast as.
	/// </summary>
	Feed = 5,

	/// <summary>
	/// The program guides a channel or feed can be grabbed from.
	/// </summary>
	Guide = 6,

	/// <summary>
	/// The logos of the channels and feeds.
	/// </summary>
	Logo = 7,

	/// <summary>
	/// The streams of the channels and feeds.
	/// </summary>
	Stream = 8
}
