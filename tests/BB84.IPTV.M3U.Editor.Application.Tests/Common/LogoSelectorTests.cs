// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

[TestClass]
public sealed class LogoSelectorTests
{
	[TestMethod]
	public void ShouldPreferTheLogoOfTheFeed()
	{
		LogoEntity[] logos =
		[
			CreateLogo("channel", null, "PNG", []),
			CreateLogo("feed", "HD", "PNG", [])
		];

		LogoEntity? selected = LogoSelector.Select(logos, "HD");

		Assert.AreEqual("feed", selected?.Url);
	}

	[TestMethod]
	public void ShouldPreferTheLogoWithoutSpecialTags()
	{
		LogoEntity[] logos =
		[
			CreateLogo("dark", null, "PNG", ["dark"]),
			CreateLogo("plain", null, "PNG", [])
		];

		LogoEntity? selected = LogoSelector.Select(logos);

		Assert.AreEqual("plain", selected?.Url);
	}

	[TestMethod]
	public void ShouldPreferTheFormatThatIsShownBest()
	{
		LogoEntity[] logos =
		[
			CreateLogo("gif", null, "GIF", []),
			CreateLogo("png", null, "PNG", [])
		];

		LogoEntity? selected = LogoSelector.Select(logos);

		Assert.AreEqual("png", selected?.Url);
	}

	[TestMethod]
	public void ShouldPreferTheLargerImageOfTheSameKind()
	{
		LogoEntity[] logos =
		[
			CreateLogo("small", null, "PNG", [], 64),
			CreateLogo("large", null, "PNG", [], 512)
		];

		LogoEntity? selected = LogoSelector.Select(logos);

		Assert.AreEqual("large", selected?.Url);
	}

	[TestMethod]
	public void ShouldTakeAnSvgWhenThereIsNothingElse()
	{
		LogoEntity[] logos = [CreateLogo("svg", null, "SVG", [])];

		LogoEntity? selected = LogoSelector.Select(logos);

		Assert.AreEqual("svg", selected?.Url);
	}

	[TestMethod]
	public void ShouldReturnNothingWithoutLogos()
		=> Assert.IsNull(LogoSelector.Select([]));

	private static LogoEntity CreateLogo(string url, string? feed, string format, string[] tags, float size = 256) => new()
	{
		Channel = "DasErste.de",
		Feed = feed,
		Url = url,
		Format = format,
		Tags = tags,
		Width = size,
		Height = size
	};
}