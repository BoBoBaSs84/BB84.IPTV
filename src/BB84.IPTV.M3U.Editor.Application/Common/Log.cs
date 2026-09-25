// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Holds every entry the application writes to the log.
/// </summary>
/// <remarks>
/// The entries are source generated from the <c>LoggerMessage</c> attribute, so no class declares a
/// logging delegate of its own. They are split by category into the <c>Log.*.cs</c> files beside
/// this one and their event ids come from <see cref="LogEvents"/>.
/// </remarks>
public static partial class Log
{
}
