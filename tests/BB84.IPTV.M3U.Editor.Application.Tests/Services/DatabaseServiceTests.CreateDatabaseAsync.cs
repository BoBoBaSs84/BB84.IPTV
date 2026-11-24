// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.EntityFrameworkCore.Repositories.Abstractions;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Persistence.Repositories;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Domain.Entities;

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
	public async Task CreateDatabaseAsyncShouldDeleteTheCachedLogoFilesAfterTheReset()
	{
		CancellationToken token = CancellationToken.None;
		MockSequence sequence = new();
		Mock<ILogoRepository> logoRepositoryMock = new();
		List<string?> deletedPaths = [];
		_repositoryServiceMock.Setup(r => r.Logos).Returns(logoRepositoryMock.Object);
		logoRepositoryMock.InSequence(sequence).Setup(r => r.GetListAsync(It.IsAny<Query<LogoEntity>>(), token))
			.ReturnsAsync([CreateLogo("/logos/a.png"), CreateLogo("/logos/b.png")]);
		_repositoryServiceMock.InSequence(sequence).Setup(r => r.ResetCatalogAsync(token)).ReturnsAsync(0);
		_logoStoreServiceMock.InSequence(sequence).Setup(s => s.Clear(It.IsAny<IEnumerable<string?>>()))
			.Callback((IEnumerable<string?> paths) => deletedPaths.AddRange(paths))
			.Returns(2);

		bool result = await _sut.CreateDatabaseAsync(token)
			.ConfigureAwait(false);

		Assert.IsTrue(result);
		Assert.AreSequenceEqual(["/logos/a.png", "/logos/b.png"], deletedPaths);
		_eventServiceMock.Verify(e => e.Publish(It.IsAny<LogoCacheChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task CreateDatabaseAsyncShouldKeepTheCachedLogoFilesWhenTheResetFails()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.ResetCatalogAsync(token)).ThrowsAsync(new InvalidOperationException());

		await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateDatabaseAsync(token))
			.ConfigureAwait(false);

		_logoStoreServiceMock.Verify(s => s.Clear(It.IsAny<IEnumerable<string?>>()), Times.Never);
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

	private static LogoEntity CreateLogo(string localPath)
		=> new() { Channel = "DasErste.de", Tags = [], Url = "https://example.com/logo.png", LocalPath = localPath };
}