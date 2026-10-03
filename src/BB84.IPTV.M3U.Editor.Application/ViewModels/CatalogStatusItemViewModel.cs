// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents one row of the catalog status: when a list was imported and what the last run did.
/// </summary>
/// <remarks>
/// The times are stored as UTC and shown in the time zone of the user. A list that was never read
/// shows the localized text for never and no counters.
/// </remarks>
/// <param name="status">What is known about the list.</param>
public sealed class CatalogStatusItemViewModel(CatalogStatusResponse status)
{
	/// <summary>
	/// Gets the name of the list of the catalog.
	/// </summary>
	public string Kind { get; } = status.Kind.GetDisplayName();

	/// <summary>
	/// Gets when the list was read for the first time.
	/// </summary>
	public string FirstImported { get; } = Format(status.FirstImported);

	/// <summary>
	/// Gets when the list was read the last time.
	/// </summary>
	public string LastChecked { get; } = Format(status.LastChecked);

	/// <summary>
	/// Gets when the list brought a change the last time.
	/// </summary>
	public string LastChanged { get; } = Format(status.LastChanged);

	/// <summary>
	/// Gets how many rows the last run added.
	/// </summary>
	public string Added { get; } = Format(status.Added, status.IsImported);

	/// <summary>
	/// Gets how many rows the last run updated.
	/// </summary>
	public string Updated { get; } = Format(status.Updated, status.IsImported);

	/// <summary>
	/// Gets how many rows the last run removed.
	/// </summary>
	public string Removed { get; } = Format(status.Removed, status.IsImported);

	private static string Format(DateTime? value)
		=> value.HasValue
			? value.Value.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
			: Resources.CatalogStatusNever;

	private static string Format(int value, bool isImported)
		=> isImported ? value.ToString(CultureInfo.CurrentCulture) : string.Empty;
}
