// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// The two columns of a guide the site list is built from.
/// </summary>
/// <param name="Site">The domain name of the site.</param>
/// <param name="Channel">The iptv-org channel of the guide, if it names one.</param>
internal sealed record SiteChannel(string Site, string? Channel);
