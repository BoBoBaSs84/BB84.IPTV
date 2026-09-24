using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;

using Moq;

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

	[TestMethod]
	public async Task ImportDatabaseAsyncShouldReportItsProgressOncePerStep()
	{
		await _sut.ImportDatabaseAsync(CancellationToken.None)
			.ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<DatabaseImportProgressEvent>()), Times.Exactly(8));

		// The status bar of the main window follows the import, like it follows the logo cache.
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ProgressChangedEvent>()), Times.Exactly(8));
		_eventServiceMock.Verify(x => x.Publish(It.Is<ProgressChangedEvent>(e => e.Value == 100)), Times.Once);
	}
}
