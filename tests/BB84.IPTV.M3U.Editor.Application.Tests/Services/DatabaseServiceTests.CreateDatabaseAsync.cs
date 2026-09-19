using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class DatabaseServiceTests
{
	[TestMethod]
	public async Task CreateDatabaseAsyncShouldMigrateAndResetCatalog()
	{
		CancellationToken token = CancellationToken.None;
		MockSequence sequence = new();
		_repositoryServiceMock.InSequence(sequence).Setup(r => r.MigrateDatabaseAsync(token)).Returns(Task.CompletedTask);
		_repositoryServiceMock.InSequence(sequence).Setup(r => r.ResetCatalogAsync(token)).ReturnsAsync(0);

		bool result = await _sut.CreateDatabaseAsync(token)
			.ConfigureAwait(false);

		Assert.IsTrue(result);
		_repositoryServiceMock.Verify(r => r.MigrateDatabaseAsync(token), Times.Once);
		_repositoryServiceMock.Verify(r => r.ResetCatalogAsync(token), Times.Once);
	}

	[TestMethod]
	public async Task CreateDatabaseAsyncShouldPropagateMigrationErrors()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.MigrateDatabaseAsync(token)).ThrowsAsync(new InvalidOperationException());

		await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateDatabaseAsync(token))
			.ConfigureAwait(false);

		_repositoryServiceMock.Verify(r => r.ResetCatalogAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task MigrateDatabaseAsyncShouldMigrateOnly()
	{
		CancellationToken token = CancellationToken.None;

		await _sut.MigrateDatabaseAsync(token)
			.ConfigureAwait(false);

		_repositoryServiceMock.Verify(r => r.MigrateDatabaseAsync(token), Times.Once);
		_repositoryServiceMock.Verify(r => r.ResetCatalogAsync(It.IsAny<CancellationToken>()), Times.Never);
	}
}