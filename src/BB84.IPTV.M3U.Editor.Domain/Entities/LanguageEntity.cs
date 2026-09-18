using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a language with its name and code, used for categorizing channels
/// in the IPTV M3U editor application.
/// </summary>
public sealed class LanguageEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the name of the language, such as "English" or "Spanish". This property
	/// is used to identify the language in the IPTV M3U editor application.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the code of the language, such as "en" for English or "es" for Spanish.
	/// This property is used to identify the language in the IPTV M3U editor application.
	/// </summary>
	public required string Code { get; set; }
}
