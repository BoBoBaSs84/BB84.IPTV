// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Searches a real SQLite catalog through <see cref="ICatalogService"/>.
/// </summary>
[TestClass]
public sealed class CatalogSearchTests
{
	private SqliteTestDatabase _database = default!;
	private ICatalogService _sut = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);
		_sut = _database.Services.GetRequiredService<ICatalogService>();

		await SeedAsync().ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task SearchAsyncShouldFindByNameAndSkipChannelsWithoutStream()
	{
		IReadOnlyList<CatalogChannelResponse> channels = await _sut
			.SearchAsync(new CatalogSearchRequest { SearchText = "Erste" })
			.ConfigureAwait(false);

		Assert.HasCount(1, channels);
		Assert.AreEqual("DasErste.de", channels[0].Channel);
		Assert.AreEqual("https://example.com/ard-1080.m3u8", channels[0].StreamUrl);
		Assert.AreEqual("1080p", channels[0].Quality);
		Assert.AreEqual("https://logo.example/ard.png", channels[0].LogoUrl);
		Assert.AreSequenceEqual(["deu"], channels[0].Languages);
	}

	[TestMethod]
	public async Task SearchAsyncShouldReturnChannelsWithoutStreamWhenAsked()
	{
		IReadOnlyList<CatalogChannelResponse> channels = await _sut
			.SearchAsync(new CatalogSearchRequest { SearchText = "Offline", IncludeWithoutStream = true })
			.ConfigureAwait(false);

		Assert.HasCount(1, channels);
		Assert.IsNull(channels[0].StreamUrl);
	}

	[TestMethod]
	public async Task SearchAsyncShouldFilterByCountryLanguageAndCategory()
	{
		IReadOnlyList<CatalogChannelResponse> byCountry = await _sut
			.SearchAsync(new CatalogSearchRequest { Country = "FR" })
			.ConfigureAwait(false);

		IReadOnlyList<CatalogChannelResponse> byLanguage = await _sut
			.SearchAsync(new CatalogSearchRequest { Language = "fra" })
			.ConfigureAwait(false);

		IReadOnlyList<CatalogChannelResponse> byCategory = await _sut
			.SearchAsync(new CatalogSearchRequest { Category = "news" })
			.ConfigureAwait(false);

		Assert.HasCount(1, byCountry);
		Assert.AreEqual("France24.fr", byCountry[0].Channel);
		Assert.HasCount(1, byLanguage);
		Assert.AreEqual("France24.fr", byLanguage[0].Channel);
		Assert.HasCount(2, byCategory);
	}

	[TestMethod]
	public async Task SearchAsyncShouldSkipNsfwChannelsUnlessAsked()
	{
		IReadOnlyList<CatalogChannelResponse> without = await _sut
			.SearchAsync(new CatalogSearchRequest())
			.ConfigureAwait(false);

		IReadOnlyList<CatalogChannelResponse> with = await _sut
			.SearchAsync(new CatalogSearchRequest { IncludeNsfw = true })
			.ConfigureAwait(false);

		Assert.IsFalse(without.Any(channel => channel.IsNsfw));
		Assert.IsTrue(with.Any(channel => channel.IsNsfw));
	}

	[TestMethod]
	public async Task SearchAsyncShouldLimitTheNumberOfResults()
	{
		IReadOnlyList<CatalogChannelResponse> channels = await _sut
			.SearchAsync(new CatalogSearchRequest { MaxResults = 1 })
			.ConfigureAwait(false);

		Assert.HasCount(1, channels);
	}

	[TestMethod]
	public async Task GetFiltersAsyncShouldReturnTheImportedValues()
	{
		CatalogFilterResponse filters = await _sut
			.GetFiltersAsync()
			.ConfigureAwait(false);

		Assert.HasCount(2, filters.Countries);
		Assert.AreEqual("France", filters.Countries[0].Name);
		Assert.AreEqual("Germany", filters.Countries[1].Name);
		Assert.HasCount(2, filters.Languages);
		Assert.HasCount(2, filters.Categories);
	}

	private async Task SeedAsync()
		=> await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Countries.CreateAsync(
			[
				new CountryEntity { Code = "DE", Name = "Germany", Languages = ["deu"] },
				new CountryEntity { Code = "FR", Name = "France", Languages = ["fra"] }
			]).ConfigureAwait(false);

			await repository.Languages.CreateAsync(
			[
				new LanguageEntity { Code = "deu", Name = "German" },
				new LanguageEntity { Code = "fra", Name = "French" }
			]).ConfigureAwait(false);

			await repository.Categories.CreateAsync(
			[
				new CategoryEntity { Category = "news", Name = "News", Description = "News channels" },
				new CategoryEntity { Category = "xxx", Name = "XXX", Description = "Adult channels" }
			]).ConfigureAwait(false);

			await repository.Channels.CreateAsync(
			[
				CreateChannel("DasErste.de", "Das Erste", "DE", ["news"]),
				CreateChannel("France24.fr", "France 24", "FR", ["news"]),
				CreateChannel("Offline.de", "Offline TV", "DE", []),
				CreateChannel("Adult.de", "Adult TV", "DE", ["xxx"], isNsfw: true)
			]).ConfigureAwait(false);

			await repository.Feeds.CreateAsync(
			[
				CreateFeed("DasErste.de", "SD", ["deu"]),
				CreateFeed("France24.fr", "SD", ["fra"]),
				CreateFeed("Adult.de", "SD", ["deu"])
			]).ConfigureAwait(false);

			await repository.Streams.CreateAsync(
			[
				new StreamEntity { Channel = "DasErste.de", Title = "Das Erste", Url = "https://example.com/ard-720.m3u8", Quality = "720p" },
				new StreamEntity { Channel = "DasErste.de", Title = "Das Erste", Url = "https://example.com/ard-1080.m3u8", Quality = "1080p" },
				new StreamEntity { Channel = "France24.fr", Title = "France 24", Url = "https://example.com/france24.m3u8" },
				new StreamEntity { Channel = "Adult.de", Title = "Adult TV", Url = "https://example.com/adult.m3u8" }
			]).ConfigureAwait(false);

			await repository.Logos.CreateAsync(
			[
				new LogoEntity { Channel = "DasErste.de", Url = "https://logo.example/ard.png", Tags = [], Format = "PNG" }
			]).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);

	private static ChannelEntity CreateChannel(string channel, string name, string country, string[] categories, bool isNsfw = false) => new()
	{
		Channel = channel,
		Name = name,
		Country = country,
		Categories = categories,
		IsNsfw = isNsfw,
		AltNames = [],
		Owners = []
	};

	private static FeedEntity CreateFeed(string channel, string feed, string[] languages) => new()
	{
		Channel = channel,
		Feed = feed,
		Name = feed,
		Format = "1080p",
		IsMain = true,
		Languages = languages,
		AltNames = [],
		BroadcastArea = [],
		Timezones = []
	};
}
