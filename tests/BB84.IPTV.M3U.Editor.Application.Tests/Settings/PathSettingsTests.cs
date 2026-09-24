// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.ComponentModel;

using BB84.IPTV.M3U.Editor.Application.Settings;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Settings;

[TestClass]
public sealed class PathSettingsTests
{
	[TestMethod]
	public void EveryPathShouldBeEmptyByDefault()
	{
		PathSettings sut = new();

		Assert.AreEqual(string.Empty, sut.DataDirectory);
		Assert.AreEqual(string.Empty, sut.LogoDirectory);
		Assert.AreEqual(string.Empty, sut.LogDirectory);
	}

	[TestMethod]
	public void ChangingAPathShouldRaisePropertyChanged()
	{
		PathSettings sut = new();
		List<string> changed = [];
		sut.PropertyChanged += (sender, args) => changed.Add(args.PropertyName ?? string.Empty);

		sut.DataDirectory = Path.GetTempPath();
		sut.LogoDirectory = Path.GetTempPath();
		sut.LogDirectory = Path.GetTempPath();

		Assert.AreEqual(3, changed.Count);
		Assert.Contains(nameof(PathSettings.DataDirectory), changed);
		Assert.Contains(nameof(PathSettings.LogoDirectory), changed);
		Assert.Contains(nameof(PathSettings.LogDirectory), changed);
	}

	[TestMethod]
	public void NullShouldBeKeptAsAnEmptyPath()
	{
		PathSettings sut = new()
		{
			DataDirectory = null!,
			LogoDirectory = null!,
			LogDirectory = null!
		};

		Assert.AreEqual(string.Empty, sut.DataDirectory);
		Assert.AreEqual(string.Empty, sut.LogoDirectory);
		Assert.AreEqual(string.Empty, sut.LogDirectory);
	}

	[TestMethod]
	public void ApplicationSettingsShouldReadTheWrittenPaths()
	{
		ApplicationSettings settings = new();
		settings.Paths.DataDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-data");
		settings.Paths.LogoDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-logos");
		settings.Paths.LogDirectory = Path.Combine(Path.GetTempPath(), "bb84-iptv-logs");

		ApplicationSettings result = ApplicationSettings.Read(ApplicationSettings.Write(settings));

		Assert.AreEqual(settings.Paths.DataDirectory, result.Paths.DataDirectory);
		Assert.AreEqual(settings.Paths.LogoDirectory, result.Paths.LogoDirectory);
		Assert.AreEqual(settings.Paths.LogDirectory, result.Paths.LogDirectory);
	}
}