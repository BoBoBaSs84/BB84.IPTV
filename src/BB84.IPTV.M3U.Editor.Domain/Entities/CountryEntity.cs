using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a country with its name, code, languages and flag.
/// </summary>
public sealed class CountryEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the ISO 3166-1 alpha-2 code of the country.
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// Gets or sets the name of the country.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the list of official languages of the country (ISO 639-3 code).
	/// </summary>
	public ICollection<string> Languages { get; set; } = [];

	/// <summary>
	/// Gets or sets the country flag emoji.
	/// </summary>
	public string? Flag { get; set; }
}
