// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Linq.Expressions;

using BB84.EntityFrameworkCore.Entities.Abstractions;
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories.Base;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that searches the imported iptv-org channel catalog.
/// </summary>
/// <remarks>
/// Categories and languages are stored as <c>JSON</c> collections, which the database cannot filter,
/// so they are matched in memory on the channels the database has already narrowed down.
/// </remarks>
/// <param name="serviceScopeFactory">The scope factory used to resolve the scoped repository service per call.</param>
internal sealed class CatalogService(IServiceScopeFactory serviceScopeFactory) : ICatalogService
{
	/// <summary>
	/// The number of identifiers per <c>IN</c> clause, so the parameter limit of SQLite is never reached.
	/// </summary>
	private const int ChunkSize = 500;

	public async Task<CatalogFilterResponse> GetFiltersAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<CatalogFilterValue> countries = await repositoryService.Countries
			.GetListAsync(
				c => new CatalogFilterValue(c.Code, c.Name),
				new Query<CountryEntity> { OrderBy = q => q.OrderBy(c => c.Name) },
				cancellationToken)
			.ConfigureAwait(false);

		IReadOnlyList<CatalogFilterValue> languages = await repositoryService.Languages
			.GetListAsync(
				l => new CatalogFilterValue(l.Code, l.Name),
				new Query<LanguageEntity> { OrderBy = q => q.OrderBy(l => l.Name) },
				cancellationToken)
			.ConfigureAwait(false);

		IReadOnlyList<CatalogFilterValue> categories = await repositoryService.Categories
			.GetListAsync(
				c => new CatalogFilterValue(c.Category, c.Name),
				new Query<CategoryEntity> { OrderBy = q => q.OrderBy(c => c.Name) },
				cancellationToken)
			.ConfigureAwait(false);

		return new CatalogFilterResponse
		{
			Countries = countries,
			Languages = languages,
			Categories = categories
		};
	}

	public async Task<IPagedList<CatalogChannelResponse>> SearchAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRepositoryService repositoryService = GetRepositoryService(scope);

		IReadOnlyList<ChannelEntity> channels = await repositoryService.Channels
			.GetListAsync(
				new Query<ChannelEntity> { Where = BuildChannelFilter(request), OrderBy = q => q.OrderBy(c => c.Name) },
				cancellationToken)
			.ConfigureAwait(false);

		string? category = request.Category?.Trim();
		string? language = request.Language?.Trim();
		bool filterByLanguage = !string.IsNullOrWhiteSpace(language);

		IEnumerable<ChannelEntity> candidates = string.IsNullOrWhiteSpace(category)
			? channels
			: channels.Where(channel => Contains(channel.Categories, category));

		List<string> candidateIds = [.. candidates.Select(channel => channel.Channel)];

		// Feeds and streams of every candidate are only needed for the filters, the page itself is
		// served by the smaller lookups below.
		ILookup<string, FeedEntity> feedsByChannel = filterByLanguage
			? await LoadFeedsAsync(repositoryService, candidateIds, cancellationToken).ConfigureAwait(false)
			: EmptyLookup<FeedEntity>();

		ILookup<string, StreamEntity> streamsByChannel = request.IncludeWithoutStream
			? EmptyLookup<StreamEntity>()
			: await LoadStreamsAsync(repositoryService, candidateIds, cancellationToken).ConfigureAwait(false);

		if (filterByLanguage)
		{
			candidates = candidates
				.Where(channel => feedsByChannel[channel.Channel].Any(feed => Contains(feed.Languages, language)));
		}

		if (!request.IncludeWithoutStream)
			candidates = candidates.Where(channel => streamsByChannel[channel.Channel].Any());

		List<ChannelEntity> matches = [.. candidates];
		List<ChannelEntity> page = [.. matches.Skip(request.Skip).Take(request.PageSize)];
		List<string> pageIds = [.. page.Select(channel => channel.Channel)];

		if (!filterByLanguage)
			feedsByChannel = await LoadFeedsAsync(repositoryService, pageIds, cancellationToken).ConfigureAwait(false);

		if (request.IncludeWithoutStream)
			streamsByChannel = await LoadStreamsAsync(repositoryService, pageIds, cancellationToken).ConfigureAwait(false);

		List<LogoEntity> logos = await LoadChunkedAsync(
			repositoryService.Logos,
			pageIds,
			chunk => logo => chunk.Contains(logo.Channel),
			cancellationToken).ConfigureAwait(false);

		ILookup<string, LogoEntity> logosByChannel = logos.ToLookup(logo => logo.Channel, StringComparer.OrdinalIgnoreCase);

		IEnumerable<CatalogChannelResponse> responses = page
			.Select(channel => ToResponse(channel, feedsByChannel[channel.Channel], streamsByChannel[channel.Channel], logosByChannel[channel.Channel]));

		return new PagedList<CatalogChannelResponse>(responses, matches.Count, request.PageNumber, request.PageSize);
	}

	/// <summary>
	/// Builds the part of the filter the database can apply: text, country and the NSFW flag.
	/// </summary>
	private static Expression<Func<ChannelEntity, bool>>? BuildChannelFilter(CatalogSearchRequest request)
	{
		Expression<Func<ChannelEntity, bool>>? filter = null;

		if (!string.IsNullOrWhiteSpace(request.SearchText))
		{
			string text = request.SearchText.Trim();
			filter = channel => channel.Name.Contains(text) || channel.Channel.Contains(text);
		}

		if (!string.IsNullOrWhiteSpace(request.Country))
		{
			string country = request.Country.Trim();
			filter = Combine(filter, channel => channel.Country == country);
		}

		if (!request.IncludeNsfw)
			filter = Combine(filter, channel => !channel.IsNsfw);

		return filter;
	}

	private static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>>? left, Expression<Func<T, bool>> right)
	{
		if (left is null)
			return right;

		ParameterExpression parameter = left.Parameters[0];
		Expression body = Expression.AndAlso(left.Body, new ParameterReplacer(right.Parameters[0], parameter).Visit(right.Body));

		return Expression.Lambda<Func<T, bool>>(body, parameter);
	}

	private static async Task<ILookup<string, FeedEntity>> LoadFeedsAsync(IRepositoryService repositoryService, IReadOnlyList<string> channelIds, CancellationToken cancellationToken)
	{
		List<FeedEntity> feeds = await LoadChunkedAsync(
			repositoryService.Feeds,
			channelIds,
			chunk => feed => chunk.Contains(feed.Channel),
			cancellationToken).ConfigureAwait(false);

		return feeds.ToLookup(feed => feed.Channel, StringComparer.OrdinalIgnoreCase);
	}

	private static async Task<ILookup<string, StreamEntity>> LoadStreamsAsync(IRepositoryService repositoryService, IReadOnlyList<string> channelIds, CancellationToken cancellationToken)
	{
		List<StreamEntity> streams = await LoadChunkedAsync(
			repositoryService.Streams,
			channelIds,
			chunk => stream => stream.Channel != null && chunk.Contains(stream.Channel),
			cancellationToken).ConfigureAwait(false);

		return streams.ToLookup(stream => stream.Channel!, StringComparer.OrdinalIgnoreCase);
	}

	private static ILookup<string, TEntity> EmptyLookup<TEntity>()
		=> Array.Empty<TEntity>().ToLookup(_ => string.Empty);

	/// <summary>
	/// Loads the entities of the given channels, in chunks, so the query stays within the parameter limit.
	/// </summary>
	private static async Task<List<TEntity>> LoadChunkedAsync<TEntity>(IRepositoryBase<TEntity> repository, IReadOnlyList<string> channelIds, Func<string[], Expression<Func<TEntity, bool>>> filter, CancellationToken cancellationToken)
		where TEntity : class, IIdentityEntity<int>
	{
		List<TEntity> entities = [];

		foreach (string[] chunk in channelIds.Chunk(ChunkSize))
		{
			IReadOnlyList<TEntity> loaded = await repository
				.GetListAsync(new Query<TEntity> { Where = filter(chunk) }, cancellationToken)
				.ConfigureAwait(false);

			entities.AddRange(loaded);
		}

		return entities;
	}

	private static CatalogChannelResponse ToResponse(ChannelEntity channel, IEnumerable<FeedEntity> feeds, IEnumerable<StreamEntity> streams, IEnumerable<LogoEntity> logos)
	{
		StreamEntity? stream = SelectStream(streams);
		LogoEntity? logo = SelectLogo(logos, stream?.Feed);

		return new CatalogChannelResponse
		{
			Channel = channel.Channel,
			Feed = stream?.Feed,
			Name = channel.Name,
			Country = channel.Country,
			Languages = [.. feeds.SelectMany(feed => Values(feed.Languages)).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase)],
			Categories = [.. Values(channel.Categories)],
			IsNsfw = channel.IsNsfw,
			StreamUrl = stream?.Url,
			Quality = stream?.Quality,
			LogoUrl = logo?.Url
		};
	}

	/// <summary>
	/// Picks the stream with the highest resolution, a stream without a quality counts as the lowest.
	/// </summary>
	private static StreamEntity? SelectStream(IEnumerable<StreamEntity> streams)
		=> streams
			.OrderByDescending(stream => GetResolution(stream.Quality))
			.ThenBy(stream => stream.Feed is null ? 0 : 1)
			.FirstOrDefault();

	/// <summary>
	/// Picks the logo of the feed of the selected stream, otherwise the logo of the channel itself.
	/// </summary>
	private static LogoEntity? SelectLogo(IEnumerable<LogoEntity> logos, string? feed)
		=> logos.FirstOrDefault(logo => feed is not null && feed.Equals(logo.Feed, StringComparison.OrdinalIgnoreCase))
			?? logos.FirstOrDefault(logo => logo.Feed is null)
			?? logos.FirstOrDefault();

	private static int GetResolution(string? quality)
	{
		if (string.IsNullOrWhiteSpace(quality))
			return 0;

		string digits = new([.. quality.TakeWhile(char.IsDigit)]);

		return int.TryParse(digits, out int resolution) ? resolution : 0;
	}

	private static bool Contains(IEnumerable<string>? values, string? value)
		=> value is not null && values is not null && values.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// An empty collection is stored as <see langword="null"/>, and the value converter is not used for
	/// <see langword="null"/>, so a collection read from the database can be <see langword="null"/>.
	/// </summary>
	private static IEnumerable<string> Values(ICollection<string>? values)
		=> values ?? [];

	private static IRepositoryService GetRepositoryService(IServiceScope scope)
		=> scope.ServiceProvider.GetRequiredService<IRepositoryService>();

	/// <summary>
	/// Replaces the parameter of a lambda, so two filters can share one parameter.
	/// </summary>
	private sealed class ParameterReplacer(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
	{
		protected override Expression VisitParameter(ParameterExpression node)
			=> ReferenceEquals(node, source) ? target : base.VisitParameter(node);
	}
}