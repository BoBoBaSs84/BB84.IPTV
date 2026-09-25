// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository abstraction for managing <see cref="GuideEntity"/> instances.
/// </summary>
public interface IGuideRepository : IRepositoryBase<GuideEntity>
{ }
