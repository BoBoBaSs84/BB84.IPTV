// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Settings.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Settings;

/// <summary>
/// Represents the data path settings of the application, which say where the database, the cached
/// logos and the log files are kept.
/// </summary>
/// <remarks>
/// An empty value means the default location below the per-user data directory. A changed path is
/// used after a restart, because the database, the logger and the logo store are set up while the
/// application starts. The settings file itself always stays in the default data directory, it is
/// what tells the application where everything else lives.
/// </remarks>
public sealed class PathSettings : SettingsBase
{
	private string _dataDirectory = string.Empty;
	private string _logoDirectory = string.Empty;
	private string _logDirectory = string.Empty;

	/// <summary>
	/// Gets or sets the directory that holds the database, empty for the per-user data directory.
	/// </summary>
	[GenerateIniFileValue]
	public string DataDirectory { get => _dataDirectory; set => SetProperty(ref _dataDirectory, value ?? string.Empty); }

	/// <summary>
	/// Gets or sets the directory that holds the cached logos, empty for <c>logos</c> below the data directory.
	/// </summary>
	[GenerateIniFileValue]
	public string LogoDirectory { get => _logoDirectory; set => SetProperty(ref _logoDirectory, value ?? string.Empty); }

	/// <summary>
	/// Gets or sets the directory that holds the log files, empty for <c>logs</c> below the data directory.
	/// </summary>
	[GenerateIniFileValue]
	public string LogDirectory { get => _logDirectory; set => SetProperty(ref _logDirectory, value ?? string.Empty); }
}