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
/// Represents a provider for directory-related operations. This class serves as
/// a wrapper around the <see cref="Directory"/> class, providing an abstraction
/// layer for directory operations within the application.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This class is a simple wrapper around the System.IO.Directory class.")]
[GenerateAbstraction(typeof(Directory), typeof(IDirectoryProvider), typeof(DirectoryProvider))]
internal sealed partial class DirectoryProvider : IDirectoryProvider
{ }
