using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class DatabaseServiceTests
{
	[TestMethod]
	public async Task CreateDatabaseAsyncShouldReturnTrueWhenRepositoryCreatesDatabaseSuccessfully()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.CreateDatabaseAsync(token)).ReturnsAsync(true);

		bool result = await _sut.CreateDatabaseAsync(token)
			.ConfigureAwait(false);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task CreateDatabaseAsyncShouldReturnFalseWhenRepositoryFailsToCreateDatabase()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.CreateDatabaseAsync(token)).ReturnsAsync(false);

		bool result = await _sut.CreateDatabaseAsync(token)
			.ConfigureAwait(false);

		Assert.IsFalse(result);
	}
}
