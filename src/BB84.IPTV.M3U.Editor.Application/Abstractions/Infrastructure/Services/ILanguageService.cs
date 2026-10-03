// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

/// <summary>
/// Represents the language the user interface is shown in.
/// </summary>
/// <remarks>
/// The language is applied once, while the application starts, from the <c>General</c> settings
/// section. The views take their texts from the localized resources when they are loaded, so a
/// language the settings name later is used after a restart, which
/// <see cref="HasPendingChanges"/> reports.
/// </remarks>
public interface ILanguageService
{
	/// <summary>
	/// The language the user interface is shown in, <see langword="null"/> for as long as no
	/// language has been applied.
	/// </summary>
	Language? CurrentLanguage { get; }

	/// <summary>
	/// Applies <paramref name="language"/> to the culture the resources are read with and keeps
	/// it as the <see cref="CurrentLanguage"/>.
	/// </summary>
	/// <param name="language">The language to apply.</param>
	void ApplyLanguage(Language language);

	/// <summary>
	/// Indicates whether the language the settings name differs from the one in use, which takes
	/// a restart to apply.
	/// </summary>
	/// <returns><see langword="true"/> if a restart is needed to use the configured language.</returns>
	bool HasPendingChanges();
}
