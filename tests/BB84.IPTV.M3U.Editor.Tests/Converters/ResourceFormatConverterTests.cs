// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Converters;

namespace BB84.IPTV.M3U.Editor.Tests.Converters;

[TestClass]
public sealed class ResourceFormatConverterTests
{
	private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

	[TestMethod]
	public void TheValueShouldBeWrittenIntoTheFormat()
	{
		object? converted = ResourceFormatConverter.Instance
			.Convert(12, typeof(string), "{0} entries", English);

		Assert.AreEqual("12 entries", converted);
	}

	[TestMethod]
	public void TheFormatShouldUseTheCultureOfTheBinding()
	{
		object? converted = ResourceFormatConverter.Instance
			.Convert(1234.5m, typeof(string), "{0}", CultureInfo.GetCultureInfo("de-DE"));

		Assert.AreEqual("1234,5", converted);
	}

	[TestMethod]
	public void WithoutAFormatTheValueShouldStayAsItIs()
	{
		object? converted = ResourceFormatConverter.Instance
			.Convert(12, typeof(string), null, English);

		Assert.AreEqual(12, converted);
	}

	[TestMethod]
	public void ConvertingBackIsNotSupported()
		=> _ = Assert.ThrowsExactly<NotSupportedException>(()
			=> ResourceFormatConverter.Instance.ConvertBack("12 entries", typeof(int), null, English));
}
