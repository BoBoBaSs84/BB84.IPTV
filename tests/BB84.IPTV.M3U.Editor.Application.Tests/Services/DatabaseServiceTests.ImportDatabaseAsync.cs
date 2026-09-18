using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class DatabaseServiceTests
{
	[TestMethod]
	public async Task ImportDatabaseAsyncShouldImportAllDataSuccessfully()
	{
		CancellationToken token = CancellationToken.None;

		DatabaseImportResponse response = await _sut
			.ImportDatabaseAsync(token)
			.ConfigureAwait(false);

		Assert.IsNotNull(response);
		Assert.IsFalse(response.IsSuccess);
		Assert.AreEqual(0, response.CategoriesImported);
		Assert.AreEqual(0, response.CountriesImported);
		Assert.AreEqual(0, response.LanguagesImported);
		Assert.AreEqual(0, response.ChannelsImported);
		Assert.AreEqual(0, response.FeedsImported);
		Assert.AreEqual(0, response.GuidesImported);
		Assert.AreEqual(0, response.LogosImported);
		Assert.AreEqual(0, response.StreamsImported);
	}
}
