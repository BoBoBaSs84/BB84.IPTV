// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Avalonia.Markup.Xaml;

namespace BB84.IPTV.M3U.Editor.Markup;

/// <summary>
/// Provides the values of an enumeration, e.g. as the items of a combo box.
/// Replaces the WPF <c>ObjectDataProvider</c> calling <see cref="Enum.GetValues(Type)"/>.
/// </summary>
/// <param name="enumType">The enumeration type whose values are provided.</param>
public sealed class EnumValuesExtension(Type enumType) : MarkupExtension
{
	/// <summary>
	/// Gets the enumeration type whose values are provided.
	/// </summary>
	public Type EnumType { get; } = enumType;

	/// <inheritdoc/>
	public override object ProvideValue(IServiceProvider serviceProvider)
		=> Enum.GetValues(EnumType);
}