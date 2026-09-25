// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BB84.IPTV.M3U.Editor.Domain.Models;

/// <summary>
/// Represents a collection of string values, where each value is represented
/// as an XML element. This class is used for XML serialization and deserialization
/// of collections of strings in the context of IPTV M3U playlist editing.
/// </summary>
[XmlType(AnonymousType = true)]
[XmlRoot(nameof(Values), Namespace = "", IsNullable = false)]
public class Values
{
	/// <summary>
	/// Gets or sets the list of values contained in this collection.
	/// Each value is represented as an XML element
	/// </summary>
	[XmlElement(nameof(Value), Form = XmlSchemaForm.Unqualified, IsNullable = true)]
	public List<Value> Items { get; set; } = [];
}

/// <summary>
/// Represents a single string value, which is used as an element within
/// the <see cref="Values"/> collection.
/// </summary>
[XmlType(AnonymousType = true)]
[XmlRoot(nameof(Value), Namespace = "", IsNullable = false)]
public class Value
{
	/// <summary>
	/// Gets or sets the text of the value. This is the actual string value that
	/// is represented as an XML element within the <see cref="Values"/> collection.
	/// </summary>
	[XmlText(DataType = "string")]
	public required string Text { get; set; }
}
