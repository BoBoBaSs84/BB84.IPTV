// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Settings.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Settings;

/// <summary>
/// Represents the settings of the logo cache, which say how an export writes the <c>tvg-logo</c>
/// of an entry whose logo is cached.
/// </summary>
public sealed class LogoSettings : SettingsBase
{
	private bool _useLocalPathsOnExport = true;
	private LogoPathStyle _exportPathStyle = LogoPathStyle.Absolute;
	private int _maxParallelDownloads = 4;

	/// <summary>
	/// Gets or sets a value indicating whether an export writes the cached file instead of the URL
	/// the logo was downloaded from.
	/// </summary>
	[GenerateIniFileValue]
	public bool UseLocalPathsOnExport { get => _useLocalPathsOnExport; set => SetProperty(ref _useLocalPathsOnExport, value); }

	/// <summary>
	/// Gets or sets how the path of a cached logo is written on export.
	/// </summary>
	[GenerateIniFileValue]
	public LogoPathStyle ExportPathStyle { get => _exportPathStyle; set => SetProperty(ref _exportPathStyle, value); }

	/// <summary>
	/// Gets or sets how many logos are downloaded at once, kept between one and
	/// <see cref="LogoCacheRequest.MaxParallelLimit"/>.
	/// </summary>
	[GenerateIniFileValue]
	public int MaxParallelDownloads
	{
		get => _maxParallelDownloads;
		set => SetProperty(ref _maxParallelDownloads, Math.Clamp(value, 1, LogoCacheRequest.MaxParallelLimit));
	}
}