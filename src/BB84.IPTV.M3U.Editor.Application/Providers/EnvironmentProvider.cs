// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Providers;

/// <summary>
/// Represents the implementation for the environment provider contract, which provides access
/// to environment-related information and operations.
/// </summary>
[ExcludeFromCodeCoverage]
[GenerateAbstraction(typeof(Environment), typeof(IEnvironmentProvider), typeof(EnvironmentProvider))]
internal sealed partial class EnvironmentProvider : IEnvironmentProvider
{ }
