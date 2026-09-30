// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text;
using System.Xml;

using BB84.Extensions.Serialization;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Writes the <c>channels.xml</c> the iptv-org EPG grabber reads.
/// </summary>
/// <remarks>
/// The format is the one documented by iptv-org/epg: a <c>channels</c> root with one
/// <c>channel</c> element per channel, carrying <c>site</c>, <c>site_id</c>, <c>lang</c> and
/// <c>xmltv_id</c>, and the name as the text of the element.
/// </remarks>
internal sealed class ChannelsXmlSerializer : IChannelsXmlSerializer
{
	private static readonly XmlWriterSettings WriterSettings = new()
	{
		Encoding = new UTF8Encoding(false),
		Indent = true,
		IndentChars = "  ",
		NewLineChars = "\n",
		OmitXmlDeclaration = false
	};

	public string Serialize(IEnumerable<GuideMappingResponse> mappings)
	{
		ArgumentNullException.ThrowIfNull(mappings);

		Channels channels = new();

		// Only a mapping that names a site, an identifier on it and an XMLTV identifier can be
		// grabbed; an incomplete one is left out instead of being written as a broken entry.
		foreach (GuideMappingResponse mapping in mappings.Where(mapping => mapping.IsComplete))
		{
			Channel channel = new()
			{
				Site = mapping.Site!.Trim(),
				SiteId = mapping.SiteId!.Trim(),
				Lang = mapping.Lang?.Trim(),
				XmltvId = mapping.XmltvId!.Trim(),
				Value = string.IsNullOrWhiteSpace(mapping.DisplayName) ? mapping.Title : mapping.DisplayName.Trim()
			};

			channels.Items.Add(channel);
		}

		return channels.ToXml(settings: WriterSettings);
	}
}
