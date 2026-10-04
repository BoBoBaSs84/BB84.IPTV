// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using Avalonia.Data.Converters;

namespace BB84.IPTV.M3U.Editor.Converters;

/// <summary>
/// Writes a bound value into a localized format string, which the view passes as the converter
/// parameter.
/// </summary>
/// <remarks>
/// Replaces <c>StringFormat</c> of a binding, because that format is a literal of the view and
/// cannot be read from the resources.
/// </remarks>
public sealed class ResourceFormatConverter : IValueConverter
{
	/// <summary>
	/// Gets the instance the views use.
	/// </summary>
	public static ResourceFormatConverter Instance { get; } = new();

	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> parameter is string format
			? string.Format(culture, format, value)
			: value;

	/// <inheritdoc/>
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
