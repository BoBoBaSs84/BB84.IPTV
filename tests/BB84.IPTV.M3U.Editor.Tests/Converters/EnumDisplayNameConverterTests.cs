// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Converters;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Tests.Converters;

[TestClass]
public sealed class EnumDisplayNameConverterTests
{
	private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");
	private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

	[TestMethod]
	public void AMemberShouldConvertToItsLocalizedName()
	{
		object? converted = EnumDisplayNameConverter.Instance
			.Convert(LogLevel.Error, typeof(string), null, German);

		Assert.AreEqual("Fehler", converted);
	}

	[TestMethod]
	public void AMemberOfAnotherCultureShouldConvertToTheNameOfThatCulture()
	{
		object? converted = EnumDisplayNameConverter.Instance
			.Convert(LogLevel.Error, typeof(string), null, English);

		Assert.AreEqual("Error", converted);
	}

	[TestMethod]
	public void AMemberOfAnApplicationEnumerationShouldConvertToItsLocalizedName()
	{
		object? converted = EnumDisplayNameConverter.Instance
			.Convert(LogoPathStyle.Relative, typeof(string), null, German);

		Assert.AreEqual("Relativ", converted);
	}

	[TestMethod]
	public void AMemberWithoutAResourceShouldConvertToItsOwnName()
	{
		object? converted = EnumDisplayNameConverter.Instance
			.Convert((Language)int.MaxValue, typeof(string), null, English);

		Assert.AreEqual(int.MaxValue.ToString(CultureInfo.InvariantCulture), converted);
	}

	[TestMethod]
	public void SomethingThatIsNoEnumerationShouldStayAsItIs()
	{
		object? converted = EnumDisplayNameConverter.Instance
			.Convert("text", typeof(string), null, English);

		Assert.AreEqual("text", converted);
	}

	[TestMethod]
	public void NullShouldStayNull()
		=> Assert.IsNull(EnumDisplayNameConverter.Instance.Convert(null, typeof(string), null, English));

	[TestMethod]
	public void ConvertingBackIsNotSupported()
		=> _ = Assert.ThrowsExactly<NotSupportedException>(()
			=> EnumDisplayNameConverter.Instance.ConvertBack("Fehler", typeof(LogLevel), null, German));
}
