using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Settings;

/// <summary>
/// Represents the application settings, which can be saved to and loaded from an INI file.
/// </summary>
[GenerateIniFile]
public sealed partial class ApplicationSettings
{
	/// <summary>
	/// Gets the general settings of the application, which include various configuration options
	/// that affect the overall behavior of the application.
	/// </summary>
	[GenerateIniFileSection]
	public GeneralSettings General { get; } = new();

	/// <summary>
	/// Gets the database settings of the application, which include configuration options related
	/// to database operations, such as command timeout and maximum batch size for bulk operations.
	/// </summary>
	[GenerateIniFileSection]
	public DatabaseSettings Database { get; } = new();

	/// <summary>
	/// Gets the settings of the logo cache, which say how an export writes the logo of an entry
	/// whose logo is cached.
	/// </summary>
	[GenerateIniFileSection]
	public LogoSettings Logo { get; } = new();
}
