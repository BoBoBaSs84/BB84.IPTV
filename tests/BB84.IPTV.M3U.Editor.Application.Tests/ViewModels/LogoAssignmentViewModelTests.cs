// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class LogoAssignmentViewModelTests
{
	[TestMethod]
	public void TheRowShouldReadTheChannelAndTheFeedOfTheEntry()
	{
		EntryModel entry = new("Das Erste HD", "https://example.com/ard.m3u8")
		{
			Channel = "DasErste.de",
			Feed = "HD"
		};

		LogoAssignmentViewModel sut = new(entry);

		Assert.AreEqual("DasErste.de", sut.Channel);
		Assert.AreEqual("HD", sut.Feed);
	}

	[TestMethod]
	public void TheRowShouldReadTheChannelAndTheFeedOutOfTheTvgIdOfAnImportedEntry()
	{
		EntryModel entry = new("Das Erste HD", "https://example.com/ard.m3u8",
			metadata: new MetadataModel { TvgId = "DasErste.de@HD" });

		LogoAssignmentViewModel sut = new(entry);

		Assert.AreEqual("DasErste.de", sut.Channel);
		Assert.AreEqual("HD", sut.Feed);
	}

	[TestMethod]
	public void TheRowShouldReadTheChannelOfATvgIdWithoutAFeed()
	{
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8",
			metadata: new MetadataModel { TvgId = "DasErste.de" });

		LogoAssignmentViewModel sut = new(entry);

		Assert.AreEqual("DasErste.de", sut.Channel);
		Assert.IsNull(sut.Feed);
	}

	[TestMethod]
	public void TheRowShouldKnowNoChannelWhenTheEntryNamesNone()
	{
		LogoAssignmentViewModel sut = new(new EntryModel("Local camera", "rtsp://192.168.12.1:554"));

		Assert.IsNull(sut.Channel);
		Assert.IsNull(sut.Feed);
		Assert.IsFalse(sut.HasLogo);
	}

	[TestMethod]
	public void AssigningALogoShouldWriteItIntoTheEntry()
	{
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8");
		LogoAssignmentViewModel sut = new(entry);
		List<string> changed = [];
		sut.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

		sut.Logo = "https://logo.example/ard.png";

		Assert.AreEqual("https://logo.example/ard.png", entry.Metadata.TvgLogo);
		Assert.IsTrue(sut.HasLogo);
		Assert.Contains(nameof(LogoAssignmentViewModel.Logo), changed);
		Assert.Contains(nameof(LogoAssignmentViewModel.HasLogo), changed);
	}

	[TestMethod]
	public void AssigningTheLogoThatIsAlreadyThereShouldChangeNothing()
	{
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8",
			metadata: new MetadataModel { TvgLogo = "https://logo.example/ard.png" });
		LogoAssignmentViewModel sut = new(entry);
		List<string> changed = [];
		sut.PropertyChanged += (s, e) => changed.Add(e.PropertyName!);

		sut.Logo = "https://logo.example/ard.png";

		Assert.IsEmpty(changed);
	}

	[TestMethod]
	public void ClearingTheLogoShouldTakeItOffTheEntry()
	{
		EntryModel entry = new("Das Erste", "https://example.com/ard.m3u8",
			metadata: new MetadataModel { TvgLogo = "https://logo.example/ard.png" });
		LogoAssignmentViewModel sut = new(entry);

		sut.Logo = null;

		Assert.IsNull(entry.Metadata.TvgLogo);
		Assert.IsFalse(sut.HasLogo);
	}

	[TestMethod]
	public void TheRowShouldRefuseAnEntryThatIsNotThere()
		=> _ = Assert.ThrowsExactly<ArgumentNullException>(() => new LogoAssignmentViewModel(null!));
}
