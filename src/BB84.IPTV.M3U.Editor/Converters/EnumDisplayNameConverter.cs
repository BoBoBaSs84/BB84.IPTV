// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;

using Avalonia.Data.Converters;

using BB84.IPTV.M3U.Editor.Properties;

namespace BB84.IPTV.M3U.Editor.Converters;

/// <summary>
/// Converts an enumeration value into the name it is shown with.
/// </summary>
/// <remarks>
/// The name is read from the resources under <c>Enum.&lt;TypeName&gt;.&lt;MemberName&gt;</c>, so an
/// enumeration of another assembly needs no attribute of this application. A member the resources
/// do not name converts to its own name, which keeps a new member readable until it is translated.
/// </remarks>
public sealed class EnumDisplayNameConverter : IValueConverter
{
	/// <summary>
	/// Gets the instance the views use.
	/// </summary>
	public static EnumDisplayNameConverter Instance { get; } = new();

	/// <inheritdoc/>
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not Enum member)
			return value;

		string name = member.ToString();
		string key = $"Enum.{value.GetType().Name}.{name}";

		return Resources.ResourceManager.GetString(key, culture) ?? name;
	}

	/// <inheritdoc/>
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
