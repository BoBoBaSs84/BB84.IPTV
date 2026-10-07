// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Contracts.Queries;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Contracts.Queries;

[TestClass]
public sealed class QueryContractsTests
{
	[TestMethod]
	public void CatalogSearchQueryShouldHaveEmptyFiltersByDefault()
	{
		CatalogSearchQuery query = new();

		Assert.IsNull(query.SearchText);
		Assert.IsNull(query.Country);
		Assert.IsNull(query.Language);
		Assert.IsNull(query.Category);
		Assert.IsFalse(query.IncludeNsfw);
		Assert.IsFalse(query.IncludeWithoutStream);
	}

	[TestMethod]
	public void GuideSearchQueryShouldInitializePropertiesCorrectly()
	{
		GuideSearchQuery query = new() { SearchText = "ZDF", Site = "example.com" };

		Assert.AreEqual("ZDF", query.SearchText);
		Assert.AreEqual("example.com", query.Site);
	}

	[TestMethod]
	public void GuideSiteSearchQueryShouldInitializePropertiesCorrectly()
	{
		GuideSiteSearchQuery query = new() { Site = "example.com", SearchText = "ZDF" };

		Assert.AreEqual("example.com", query.Site);
		Assert.AreEqual("ZDF", query.SearchText);
	}

	[TestMethod]
	public void LogoSearchQueryShouldInitializePropertiesCorrectly()
	{
		LogoSearchQuery query = new() { SearchText = "ZDF", Channel = "ZDF.de", CacheState = LogoCacheFilter.Cached };

		Assert.AreEqual("ZDF", query.SearchText);
		Assert.AreEqual("ZDF.de", query.Channel);
		Assert.AreEqual(LogoCacheFilter.Cached, query.CacheState);
	}

	[TestMethod]
	public void SearchQueriesShouldClampThePagingOfTheBaseClass()
	{
		PagedQuery[] queries =
		[
			new CatalogSearchQuery { PageNumber = 0, PageSize = 1 },
			new CustomChannelSearchQuery { PageNumber = 0, PageSize = 1 },
			new GuideSearchQuery { PageNumber = 0, PageSize = 1 },
			new GuideSiteSearchQuery { Site = "example.com", PageNumber = 0, PageSize = 1 },
			new LogoSearchQuery { PageNumber = 0, PageSize = 1 },
			new PlaylistSearchQuery { PageNumber = 0, PageSize = 1 }
		];

		foreach (PagedQuery query in queries)
		{
			Assert.AreEqual(1, query.PageNumber);
			Assert.AreEqual(PagedQuery.MinPageSize, query.PageSize);
			Assert.AreEqual(0, query.Skip);
		}
	}
}
