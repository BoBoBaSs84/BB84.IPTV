// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using Avalonia.Data.Converters;

using BB84.IPTV.M3U.Editor.Services;

namespace BB84.IPTV.M3U.Editor.Converters;

/// <summary>
/// Converts the logo of an entry or of a catalog channel into the cached image.
/// </summary>
/// <remarks>
/// A logo that is not cached converts to <see langword="null"/>, so the list shows no image and
/// asks no network.
/// </remarks>
public sealed class LogoImageConverter : IValueConverter
{
	/// <summary>
	/// Gets the instance the views use.
	/// </summary>
	public static LogoImageConverter Instance { get; } = new();

	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> LogoImageService.Current?.GetImage(value as string);

	/// <inheritdoc/>
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}