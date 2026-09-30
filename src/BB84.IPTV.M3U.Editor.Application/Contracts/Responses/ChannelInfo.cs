// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

/// <summary>
/// What the catalog knows about a channel of the site browser.
/// </summary>
/// <param name="Channel">The iptv-org channel identifier.</param>
/// <param name="Name">The name of the channel.</param>
/// <param name="Country">The country code of the channel.</param>
internal sealed record ChannelInfo(string Channel, string Name, string Country);
