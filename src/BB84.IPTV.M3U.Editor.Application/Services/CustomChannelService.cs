// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that stores the user defined channels.
/// </summary>
/// <param name="serviceScopeFactory">The scope factory used to resolve the scoped repository service per call.</param>
internal sealed class CustomChannelService(IServiceScopeFactory serviceScopeFactory) : ICustomChannelService
{
	public async Task<IReadOnlyList<CustomChannelResponse>> GetChannelsAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		return await repositoryService.CustomChannels
			.GetListAsync(
				c => new CustomChannelResponse
				{
					Id = c.Id,
					Name = c.Name,
					Url = c.Url,
					GroupTitle = c.GroupTitle,
					TvgId = c.TvgId,
					TvgLogo = c.TvgLogo
				},
				new Query<CustomChannelEntity> { OrderBy = q => q.OrderBy(c => c.Name) },
				cancellationToken)
			.ConfigureAwait(false);
	}

	public async Task<int> CreateAsync(CustomChannelResponse channel, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentException.ThrowIfNullOrWhiteSpace(channel.Name);
		ArgumentException.ThrowIfNullOrWhiteSpace(channel.Url);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		CustomChannelEntity entity = new()
		{
			Name = channel.Name.Trim(),
			Url = channel.Url.Trim(),
			GroupTitle = channel.GroupTitle,
			TvgId = channel.TvgId,
			TvgLogo = channel.TvgLogo
		};

		await repositoryService.CustomChannels
			.CreateAsync(entity, cancellationToken)
			.ConfigureAwait(false);

		await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return entity.Id;
	}

	public async Task<bool> UpdateAsync(CustomChannelResponse channel, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentException.ThrowIfNullOrWhiteSpace(channel.Name);
		ArgumentException.ThrowIfNullOrWhiteSpace(channel.Url);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		CustomChannelEntity? entity = await repositoryService.CustomChannels
			.GetByIdAsync(channel.Id, new Query<CustomChannelEntity> { TrackChanges = true }, cancellationToken)
			.ConfigureAwait(false);

		if (entity is null)
			return false;

		entity.Name = channel.Name.Trim();
		entity.Url = channel.Url.Trim();
		entity.GroupTitle = channel.GroupTitle;
		entity.TvgId = channel.TvgId;
		entity.TvgLogo = channel.TvgLogo;

		await repositoryService
			.CommitChangesAsync(cancellationToken)
			.ConfigureAwait(false);

		return true;
	}

	public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		int deleted = await repositoryService.CustomChannels
			.ExecuteDeleteAsync(id, cancellationToken)
			.ConfigureAwait(false);

		return deleted > 0;
	}

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();
}
