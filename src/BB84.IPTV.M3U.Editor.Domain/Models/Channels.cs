using System.Xml.Schema;
using System.Xml.Serialization;

namespace BB84.IPTV.M3U.Editor.Domain.Models;

/// <summary>
/// Represents a collection of channels, where each channel contains information about
/// its site, language, XMLTV ID, site ID, and value.
/// </summary>
[XmlType(AnonymousType = true)]
[XmlRoot("channels", Namespace = XmlNamespace, IsNullable = false)]
public sealed class Channels
{
	private const string XmlNamespace = "";

	/// <summary>
	/// Gets or sets the list of channels contained in this collection.
	/// </summary>
	[XmlElement("channel", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
	public List<Channel> Items { get; set; } = default!;
}

/// <summary>
/// Represents a channel with attributes such as site, language, XMLTV ID, site ID, and value.
/// </summary>
[XmlType(AnonymousType = true)]
[XmlRoot("channel", Namespace = XmlNamespace, IsNullable = false)]
public sealed class Channel
{
	private const string XmlNamespace = "";

	/// <summary>
	/// Gets or sets the site associated with this channel, represented as a string attribute in the XML.
	/// </summary>
	[XmlAttribute("site", DataType = "string", Form = XmlSchemaForm.Unqualified)]
	public string Site { get; set; } = default!;

	/// <summary>
	/// Gets or sets the language associated with this channel, represented as a string attribute in the XML.
	/// </summary>
	[XmlAttribute("lang", DataType = "string", Form = XmlSchemaForm.Unqualified)]
	public string Lang { get; set; } = default!;

	/// <summary>
	/// Gets or sets the XMLTV ID associated with this channel, represented as a string attribute in the XML.
	/// </summary>
	[XmlAttribute("xmltv_id", DataType = "string", Form = XmlSchemaForm.Unqualified)]
	public string XmltvId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the site ID associated with this channel, represented as a string attribute in the XML.
	/// </summary>
	[XmlAttribute("site_id", DataType = "string", Form = XmlSchemaForm.Unqualified)]
	public string SiteId { get; set; } = default!;

	/// <summary>
	/// Gets or sets the value of this channel, represented as the text content of the XML element.
	/// </summary>
	[XmlText(DataType = "string")]
	public string Value { get; set; } = default!;
}
