// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories.Base;

using Microsoft.EntityFrameworkCore;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents the repository for managing <see cref="GuideEntity"/> instances.
/// </summary>
internal sealed class GuideRepository : RepositoryBase<GuideEntity>, IGuideRepository
{
	private readonly IDbContext _dbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuideRepository"/> class.
	/// </summary>
	/// <param name="dbContext">The database context to be used by the repository.</param>
	public GuideRepository(IDbContext dbContext) : base(dbContext)
		=> _dbContext = dbContext;

	/// <summary>
	/// The character that separates the channel from the feed in the identifier a
	/// <c>channels.xml</c> holds.
	/// </summary>
	private const char FeedSeparator = '@';

	/// <summary>
	/// The character that takes the meaning off a wildcard of a <c>LIKE</c> pattern.
	/// </summary>
	private const string EscapeCharacter = "\\";

	/// <inheritdoc/>
	/// <remarks>
	/// The query is written here instead of in the service, because it joins two entities, which a
	/// <see cref="Query{TEntity}"/> cannot express. The projection therefore stays with the query and
	/// is not a mapping expression.
	/// The text is matched with <c>LIKE</c>, which folds the case of ASCII letters only, and the
	/// order uses the binary collation of SQLite, so an upper case letter and an unknown channel
	/// come first.
	/// </remarks>
	public async Task<(IReadOnlyList<GuideOptionResponse> Guides, int TotalCount)> SearchAsync(
		string? searchText,
		string? site,
		int skip,
		int take,
		CancellationToken cancellationToken = default)
	{
		IQueryable<GuideChannel> rows = Filter(searchText, site);

		List<GuideOptionResponse> guides = await Order(rows)
			.Skip(skip)
			.Take(take)
			.Select(row => new GuideOptionResponse
			{
				Channel = row.Guide.Channel,
				Feed = row.Guide.Feed,
				Site = row.Guide.Site,
				SiteId = row.Guide.SiteId,
				SiteName = row.Guide.SiteName,
				Lang = row.Guide.Lang,
				ChannelName = row.Channel != null ? row.Channel.Name : null,
				Country = row.Channel != null ? row.Channel.Country : null
			})
			.ToListAsync(cancellationToken)
			.ConfigureAwait(false);

		// A first page that is not full holds the whole result, so it needs no counting query.
		int total = skip is 0 && guides.Count < take
			? guides.Count
			: await rows.CountAsync(cancellationToken).ConfigureAwait(false);

		return (guides, total);
	}

	/// <summary>
	/// Builds the guides the search covers, with the catalog channel each guide names.
	/// </summary>
	private IQueryable<GuideChannel> Filter(string? searchText, string? site)
	{
		// A left join, so a guide whose channel the catalog does not know is kept.
		IQueryable<GuideChannel> rows =
			from guide in _dbContext.Set<GuideEntity>().AsNoTracking()
			join channel in _dbContext.Set<ChannelEntity>().AsNoTracking()
				on guide.Channel equals channel.Channel into channels
			from channel in channels.DefaultIfEmpty()
			select new GuideChannel { Guide = guide, Channel = channel };

		if (site is not null)
			rows = rows.Where(row => row.Guide.Site == site);

		if (searchText is null)
			return rows;

		int separator = searchText.IndexOf(FeedSeparator, StringComparison.Ordinal);

		// The identifier of a channels.xml names the channel and the feed, which are two columns here.
		if (separator > 0 && separator < searchText.Length - 1)
		{
			string channelText = searchText[..separator];
			string feedText = searchText[(separator + 1)..];

			return rows.Where(row => row.Guide.Channel == channelText && row.Guide.Feed == feedText);
		}

		string pattern = string.Concat("%", Escape(searchText), "%");

		return rows.Where(row
			=> (row.Guide.Channel != null && EF.Functions.Like(row.Guide.Channel, pattern, EscapeCharacter))
			|| (row.Channel != null && EF.Functions.Like(row.Channel.Name, pattern, EscapeCharacter))
			|| EF.Functions.Like(row.Guide.Site, pattern, EscapeCharacter)
			|| EF.Functions.Like(row.Guide.SiteId, pattern, EscapeCharacter)
			|| EF.Functions.Like(row.Guide.SiteName, pattern, EscapeCharacter)
			|| EF.Functions.Like(row.Guide.Lang, pattern, EscapeCharacter));
	}

	/// <summary>
	/// Orders the guides by channel, with a unique key last.
	/// </summary>
	private static IOrderedQueryable<GuideChannel> Order(IQueryable<GuideChannel> rows)
	{
		// The identity last, so a guide never moves between pages when several share the channel.
		return rows
			.OrderBy(row => row.Guide.Channel)
			.ThenBy(row => row.Guide.Site)
			.ThenBy(row => row.Guide.SiteId)
			.ThenBy(row => row.Guide.Id);
	}

	/// <summary>
	/// Takes the meaning off the wildcards of a <c>LIKE</c> pattern, so a text of <c>_</c> does not
	/// match every guide.
	/// </summary>
	private static string Escape(string text)
		=> text
			.Replace(EscapeCharacter, string.Concat(EscapeCharacter, EscapeCharacter), StringComparison.Ordinal)
			.Replace("%", string.Concat(EscapeCharacter, "%"), StringComparison.Ordinal)
			.Replace("_", string.Concat(EscapeCharacter, "_"), StringComparison.Ordinal);

	/// <summary>
	/// Represents a guide with the catalog channel it names, which may be unknown.
	/// </summary>
	private sealed class GuideChannel
	{
		/// <summary>
		/// Gets or initializes the guide of the catalog.
		/// </summary>
		public required GuideEntity Guide { get; init; }

		/// <summary>
		/// Gets or initializes the channel the guide names, <see langword="null"/> if unknown.
		/// </summary>
		public ChannelEntity? Channel { get; init; }
	}
}
