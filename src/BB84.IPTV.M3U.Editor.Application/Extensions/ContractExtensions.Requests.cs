using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Domain.Entities;

namespace BB84.IPTV.M3U.Editor.Application.Extensions;

public static partial class ContractExtensions
{
	/// <summary>
	/// Converts a <see cref="CategoryRequest"/> to a <see cref="CategoryEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static CategoryEntity ToEntity(this CategoryRequest request)
	{
		return new CategoryEntity
		{
			Category = request.Id,
			Name = request.Name,
			Description = request.Description
		};
	}

	/// <summary>
	/// Converts a <see cref="ChannelRequest"/> to a <see cref="ChannelEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static ChannelEntity ToEntity(this ChannelRequest request)
	{
		return new ChannelEntity
		{
			AltNames = [.. request.AltNames],
			Categories = [.. request.Categories],
			Channel = request.Id,
			Closed = request.Closed,
			Country = request.Country,
			IsNsfw = request.IsNsfw,
			Launched = request.Launched,
			Name = request.Name,
			Network = request.Network,
			Owners = [.. request.Owners],
			ReplacedBy = request.ReplacedBy,
			Website = request.Website
		};
	}

	/// <summary>
	/// Converts a <see cref="CountryRequest"/> to a <see cref="CountryEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static CountryEntity ToEntity(this CountryRequest request)
	{
		return new CountryEntity
		{
			Code = request.Code,
			Name = request.Name,
			Languages = [.. request.Languages],
			Flag = request.Flag
		};
	}

	/// <summary>
	/// Converts a <see cref="FeedRequest"/> to a <see cref="FeedEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static FeedEntity ToEntity(this FeedRequest request)
	{
		return new FeedEntity
		{
			Channel = request.Channel,
			Feed = request.Id,
			Name = request.Name,
			AltNames = [.. request.AltNames],
			IsMain = request.IsMain,
			BroadcastArea = [.. request.BroadcastArea],
			Timezones = [.. request.Timezones],
			Languages = [.. request.Languages],
			Format = request.Format
		};
	}

	/// <summary>
	/// Converts a <see cref="GuideRequest"/> to a <see cref="GuideEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static GuideEntity ToEntity(this GuideRequest request)
	{
		return new GuideEntity
		{
			Channel = request.Channel,
			Feed = request.Feed,
			Site = request.Site,
			SiteId = request.SiteId,
			SiteName = request.SiteName,
			Lang = request.Lang
		};
	}

	/// <summary>
	/// Converts a <see cref="LanguageRequest"/> to a <see cref="LanguageEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static LanguageEntity ToEntity(this LanguageRequest request)
	{
		return new LanguageEntity
		{
			Code = request.Code,
			Name = request.Name
		};
	}

	/// <summary>
	/// Converts a <see cref="LogoRequest"/> to a <see cref="LogoEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static LogoEntity ToEntity(this LogoRequest request)
	{
		return new LogoEntity
		{
			Channel = request.Channel,
			Feed = request.Feed,
			Format = request.Format,
			Height = request.Height,
			Width = request.Width,
			Tags = [.. request.Tags],
			Url = request.Url
		};
	}

	/// <summary>
	/// Converts a <see cref="StreamRequest"/> to a <see cref="StreamEntity"/>.
	/// </summary>
	/// <param name="request">The request to convert.</param>
	/// <returns>The converted entity.</returns>
	public static StreamEntity ToEntity(this StreamRequest request)
	{
		return new StreamEntity
		{
			Channel = request.Channel,
			Feed = request.Feed,
			Title = request.Title,
			Url = request.Url,
			Referrer = request.Referrer,
			UserAgent = request.UserAgent,
			Quality = request.Quality
		};
	}
}
