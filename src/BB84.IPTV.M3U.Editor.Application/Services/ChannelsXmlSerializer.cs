// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Text;
using System.Xml;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

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
	public string Serialize(IEnumerable<GuideMappingResponse> mappings)
	{
		ArgumentNullException.ThrowIfNull(mappings);

		XmlWriterSettings settings = new()
		{
			Encoding = new UTF8Encoding(false),
			Indent = true,
			IndentChars = "  ",
			NewLineChars = "\n",
			OmitXmlDeclaration = false
		};

		StringBuilderWriter output = new();

		using (XmlWriter writer = XmlWriter.Create(output, settings))
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("channels");

			// Only a mapping that names a site, an identifier on it and an XMLTV identifier can be
			// grabbed; an incomplete one is left out instead of being written as a broken entry.
			foreach (GuideMappingResponse mapping in mappings.Where(mapping => mapping.IsComplete))
			{
				writer.WriteStartElement("channel");
				writer.WriteAttributeString("site", mapping.Site!.Trim());
				writer.WriteAttributeString("site_id", mapping.SiteId!.Trim());

				if (!string.IsNullOrWhiteSpace(mapping.Lang))
					writer.WriteAttributeString("lang", mapping.Lang.Trim());

				writer.WriteAttributeString("xmltv_id", mapping.XmltvId!.Trim());
				writer.WriteString(string.IsNullOrWhiteSpace(mapping.DisplayName) ? mapping.Title : mapping.DisplayName.Trim());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		return output.ToString();
	}

	/// <summary>
	/// Collects what the writer produces, with the encoding the declaration announces.
	/// </summary>
	/// <remarks>
	/// A <see cref="StringWriter"/> reports UTF-16, which would end up in the declaration, but the
	/// grabber expects UTF-8.
	/// </remarks>
	private sealed class StringBuilderWriter : StringWriter
	{
		public override Encoding Encoding
			=> new UTF8Encoding(false);
	}
}