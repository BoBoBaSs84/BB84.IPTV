// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Searches the logos of a real SQLite catalog, which is where the filter, the order and the page
/// are run.
/// </summary>
[TestClass]
public sealed class LogoSearchPersistenceTests
{
	private SqliteTestDatabase _database = default!;
	private ILogoService _sut = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();

		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		_sut = _database.Services.GetRequiredService<ILogoService>();

		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Channels.CreateAsync(new ChannelEntity
			{
				Channel = "DasErste.de",
				Name = "Das Erste",
				Country = "DE",
				Categories = ["general"],
				IsNsfw = false
			}).ConfigureAwait(false);

			await repository.Logos.CreateAsync(
			[
				CreateLogo("DasErste.de", feed: null, "https://logo.example/ard.png", "PNG", ["light"], localPath: "/logos/ard.png"),
				CreateLogo("DasErste.de", "HD", "https://logo.example/ard-hd.webp", "WEBP", []),
				CreateLogo("ZDF.de", feed: null, "https://logo.example/zdf.svg", "SVG", ["dark"])
			]).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task SearchLogosAsyncShouldReadEveryLogoWithTheNameOfItsChannel()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest(), TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(3, logos);
		Assert.AreEqual(3, logos.MetaData.TotalCount);

		LogoOptionResponse first = logos[0];

		Assert.AreEqual("DasErste.de", first.Channel);
		Assert.AreEqual("Das Erste", first.ChannelName, "The channel is joined by its identifier.");
		Assert.AreEqual("DE", first.Country);
		Assert.AreEqual("light", first.Tags, "The tags of a logo are read out of the JSON column.");

		// The catalog does not know the channel of the third logo, which is kept without a name.
		LogoOptionResponse unknown = logos.First(logo => logo.Channel is "ZDF.de");

		Assert.IsNull(unknown.ChannelName);
		Assert.IsNull(unknown.Country);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldFindALogoByItsTextInEveryColumnItCovers()
	{
		Assert.HasCount(2, await SearchAsync("Das Erste").ConfigureAwait(false), "The name of the channel.");
		Assert.HasCount(2, await SearchAsync("DasErste").ConfigureAwait(false), "The identifier of the channel.");
		Assert.HasCount(1, await SearchAsync("HD").ConfigureAwait(false), "The feed.");
		Assert.HasCount(1, await SearchAsync("WEBP").ConfigureAwait(false), "The format.");
		Assert.HasCount(1, await SearchAsync("zdf.svg").ConfigureAwait(false), "The URL.");
		Assert.HasCount(1, await SearchAsync("/logos/ard.png").ConfigureAwait(false), "The path of the cached file.");
		Assert.IsEmpty(await SearchAsync("_").ConfigureAwait(false), "A wildcard of a pattern is a text like any other.");

		// The text is matched with instr, like every other catalog search of the application does it.
		Assert.IsEmpty(await SearchAsync("dasERSTE").ConfigureAwait(false), "The search is case sensitive.");
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldReadEveryLogoOfTheChannelOfATvgId()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(new LogoSearchRequest { SearchText = "DasErste.de@HD" }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		// The feed only decides which logo is preferred, so the whole channel is offered.
		Assert.HasCount(2, logos);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldKeepOnlyWhatTheCacheFilterAsksFor()
	{
		IPagedList<LogoOptionResponse> cached = await _sut
			.SearchLogosAsync(new LogoSearchRequest { CacheState = LogoCacheFilter.Cached }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		IPagedList<LogoOptionResponse> missing = await _sut
			.SearchLogosAsync(new LogoSearchRequest { CacheState = LogoCacheFilter.NotCached }, TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.HasCount(1, cached);
		Assert.AreEqual("/logos/ard.png", cached[0].LocalPath);
		Assert.IsFalse(cached[0].IsCached, "The row holds a path, but the store holds no file.");
		Assert.HasCount(2, missing);
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldOrderByChannelInTheDatabase()
	{
		IPagedList<LogoOptionResponse> logos = await _sut
			.SearchLogosAsync(
				new LogoSearchRequest(),
				TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.AreSequenceEqual(["PNG", "WEBP", "SVG"], logos.Select(logo => logo.Format).ToArray());
	}

	[TestMethod]
	public async Task SearchLogosAsyncShouldReadOnlyTheAskedPage()
	{
		IPagedList<LogoOptionResponse> page = await _sut
			.SearchLogosAsync(
				new LogoSearchRequest { Channel = "DasErste.de", PageNumber = 2, PageSize = Parameters.MinPageSize },
				TestContext.CancellationToken)
			.ConfigureAwait(false);

		Assert.IsEmpty(page, "The two logos of the channel fit on the first page.");
		Assert.AreEqual(2, page.MetaData.TotalCount, "The count covers the whole result, not the page.");
	}

	private async Task<IPagedList<LogoOptionResponse>> SearchAsync(string searchText)
		=> await _sut
			.SearchLogosAsync(new LogoSearchRequest { SearchText = searchText }, TestContext.CancellationToken)
			.ConfigureAwait(false);

	private static LogoEntity CreateLogo(string channel, string? feed, string url, string format, string[] tags, string? localPath = null) => new()
	{
		Channel = channel,
		Feed = feed,
		Url = url,
		Format = format,
		Tags = [.. tags],
		Width = 512,
		Height = 512,
		LocalPath = localPath
	};

	public TestContext TestContext { get; set; }
}
