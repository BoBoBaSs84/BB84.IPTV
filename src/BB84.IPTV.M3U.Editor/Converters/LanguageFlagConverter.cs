// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.Concurrent;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using BB84.IPTV.M3U.Editor.Application.Enumerators;

namespace BB84.IPTV.M3U.Editor.Converters;

/// <summary>
/// Converts a <see cref="Language"/> into the flag icon shown beside its name.
/// </summary>
public sealed class LanguageFlagConverter : IValueConverter
{
	private static readonly ConcurrentDictionary<Language, Bitmap> Flags = new();

	/// <summary>
	/// Gets the instance the views use.
	/// </summary>
	public static LanguageFlagConverter Instance { get; } = new();

	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not Language language)
			return null;

		return Flags.GetOrAdd(language, static member =>
		{
			string file = member switch
			{
				Language.Spanish => "flag_spain",
				Language.French => "flag_france",
				Language.German => "flag_germany",
				Language.Italian => "flag_italy",
				_ => "flag_usa"
			};

			using Stream stream = AssetLoader.Open(new Uri($"avares://BB84.IPTV.M3U.Editor/Resources/{file}.ico"));
			return new Bitmap(stream);
		});
	}

	/// <inheritdoc/>
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
