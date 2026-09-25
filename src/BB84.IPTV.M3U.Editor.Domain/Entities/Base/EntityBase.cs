// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Entities;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Domain.Entities.Base;

/// <summary>
/// Represents the base class for all entities in the application, providing a
/// common identifier property and equality comparison based on the identifier.
/// </summary>
[GenerateEquality]
public abstract partial class EntityBase : IdentityEntity<int>;
