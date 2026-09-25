// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence;

/// <summary>
/// Represents the application database context interface.
/// </summary>
public interface IDatabaseContext : IDbContext
{ }
