using System.Text.Json;
using System.Text.Json.Serialization;

using BB84.Extensions.Serialization;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Converters;

/// <summary>
/// Represents a value converter that converts between an enumerable of strings and its <c>JSON</c> representation.
/// </summary>
/// <remarks>
/// If the collection is empty, it converts to null to save space in the database.
/// </remarks>
internal sealed class StringCollectionConverter : ValueConverter<ICollection<string>, string?>
{
	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		AllowDuplicateProperties = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.Strict,
	};

	/// <summary>
	/// Initializes a new instance of the <see cref="StringCollectionConverter"/> class.
	/// </summary>
	public StringCollectionConverter() : base(values => ToJson(values), value => FromJson(value))
	{ }

	/// <summary>
	/// Converts an enumerable of strings to its <c>JSON</c> representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A string containing the <c>JSON</c> representation of the enumerable.</returns>
	private static string? ToJson(ICollection<string> value)
		=> value.Count.Equals(0) ? null : value.ToJson(Options);

	/// <summary>
	/// Converts an <c>JSON</c> representation back to an enumerable of strings.
	/// </summary>
	/// <param name="value">The <c>JSON</c> string to convert.</param>
	/// <returns>A list of strings represented by the <c>JSON</c>.</returns>
	private static List<string> FromJson(string? value)
		=> value is null ? [] : value.FromJson<List<string>>(Options) ?? [];
}
