// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

public sealed partial class DatabaseServiceTests
{
	[TestMethod]
	public async Task CheckDatabaseAvailabilityAsyncShouldReturnTrueWhenRepositoryCanConnect()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.CanConnectAsync(token)).ReturnsAsync(true);

		bool result = await _sut.CheckDatabaseAvailabilityAsync(token)
			.ConfigureAwait(false);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task CheckDatabaseAvailabilityAsyncShouldReturnFalseWhenRepositoryCannotConnect()
	{
		CancellationToken token = CancellationToken.None;
		_repositoryServiceMock.Setup(r => r.CanConnectAsync(token)).ReturnsAsync(false);

		bool result = await _sut.CheckDatabaseAvailabilityAsync(token)
			.ConfigureAwait(false);

		Assert.IsFalse(result);
	}
}
