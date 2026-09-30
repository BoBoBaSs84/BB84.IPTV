// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Represents the centralized mapping class for the application layer, providing methods
/// and properties to convert between different data models and entities.
/// </summary>
/// <remarks>
/// The mappings are split by the type they map, one partial per type. Projections the database
/// runs are exposed as static <see cref="Expression{TDelegate}"/> properties, so they are built
/// once and stay translatable to SQL.
/// </remarks>
internal static partial class Mappings;
