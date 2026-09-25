// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Diagnostics.CodeAnalysis;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// The user service implementation.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class UserService : IUserService
{
	/// <inheritdoc/>
	public string Name => Environment.UserName;

	/// <inheritdoc/>
	public string Domain => Environment.UserDomainName;

	/// <inheritdoc/>
	public string Machine => Environment.MachineName;
}
