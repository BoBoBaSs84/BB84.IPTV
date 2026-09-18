using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Settings.Base;
using BB84.SourceGenerators.Attributes;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Application.Settings;

/// <summary>
/// Represents the general settings of the application, which include various configuration options
/// that affect the overall behavior of the application.
/// </summary>
public sealed partial class GeneralSettings : SettingsBase
{
	private bool _autoSave = true;
	private int _autoSaveInterval = 15;
	private bool _enableLogging;
	private LogLevel _logLevel = LogLevel.Error;
	private Language _language = Language.English;

	/// <summary>
	/// Gets or sets a value indicating whether the application should automatically save changes at regular
	/// intervals.
	/// </summary>
	[GenerateIniFileValue]
	public bool AutoSave { get => _autoSave; set => SetProperty(ref _autoSave, value); }

	/// <summary>
	/// Gets or sets the interval, in minutes, at which the application should automatically save changes if
	/// auto-saving is enabled.
	/// </summary>
	[GenerateIniFileValue]
	public int AutoSaveInterval { get => _autoSaveInterval; set => SetProperty(ref _autoSaveInterval, value); }

	/// <summary>
	/// Gets or sets a value indicating whether logging is enabled for the application, allowing for the recording
	/// of events and errors to assist with troubleshooting and monitoring.
	/// </summary>
	[GenerateIniFileValue]
	public bool EnableLogging { get => _enableLogging; set => SetProperty(ref _enableLogging, value); }

	/// <summary>
	/// Gets or sets the log level for the application, determining the severity of events that should be recorded
	/// in the log.
	/// </summary>
	[GenerateIniFileValue]
	public LogLevel LogLevel { get => _logLevel; set => SetProperty(ref _logLevel, value); }

	/// <summary>
	/// Gets or sets the language for the application, allowing users to select their preferred language for the
	/// user interface.
	/// </summary>
	[GenerateIniFileValue]
	public Language Language { get => _language; set => SetProperty(ref _language, value); }
}
