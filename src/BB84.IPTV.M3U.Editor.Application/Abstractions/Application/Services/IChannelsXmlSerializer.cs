// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents the serializer that writes the <c>channels.xml</c> of the iptv-org EPG grabber.
/// </summary>
public interface IChannelsXmlSerializer
{
	/// <summary>
	/// Writes the mappings as a <c>channels.xml</c>.
	/// </summary>
	/// <param name="mappings">The mappings to write, an incomplete one is left out.</param>
	/// <returns>The content of the file.</returns>
	string Serialize(IEnumerable<GuideMappingResponse> mappings);
}