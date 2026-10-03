// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Services;

[TestClass]
public sealed class CatalogUpdateServiceTests
{
	private static readonly DateTime Now = new(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc);

	private readonly Mock<IDatabaseService> _databaseServiceMock = new();
	private readonly Mock<INotificationService> _notificationServiceMock = new();
	private readonly Mock<IProviderService> _providerServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly ApplicationSettings _settings = new();
	private readonly CatalogUpdateService _sut;

	public CatalogUpdateServiceTests()
	{
		Mock<IDateTimeProvider> dateTimeProviderMock = new();
		dateTimeProviderMock.SetupGet(x => x.UtcNow).Returns(Now);
		_providerServiceMock.SetupGet(x => x.DateTime).Returns(dateTimeProviderMock.Object);

		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CatalogSyncResponse { Kinds = [] });

		_sut = new CatalogUpdateService(
			_databaseServiceMock.Object,
			_notificationServiceMock.Object,
			_providerServiceMock.Object,
			_eventServiceMock.Object,
			_settings);
	}

	[TestMethod]
	public async Task AnIntervalOfZeroShouldSwitchTheCheckOff()
	{
		_settings.Database.UpdateIntervalDays = 0;
		SetupStatus(Now.AddYears(-1));

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsFalse(updated);
		_databaseServiceMock.Verify(x => x.GetCatalogStatusAsync(It.IsAny<CancellationToken>()), Times.Never);
		_databaseServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task AFreshCatalogShouldBeLeftAlone()
	{
		_settings.Database.UpdateIntervalDays = 7;
		SetupStatus(Now.AddDays(-2));

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsFalse(updated);
		_notificationServiceMock.Verify(x => x.ShowQuestionAsync(It.IsAny<string>()), Times.Never);
		_databaseServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task AnOldCatalogShouldBeUpdatedWhenTheUserAgrees()
	{
		_settings.Database.UpdateIntervalDays = 7;
		SetupStatus(Now.AddDays(-9));
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.Yes);

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsTrue(updated);
		_notificationServiceMock.Verify(x => x.ShowQuestionAsync(Resources.DatabaseSyncQuestion.FormatMessage(9)), Times.Once);
		_databaseServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task AnOldCatalogShouldBeLeftAloneWhenTheUserSaysNo()
	{
		_settings.Database.UpdateIntervalDays = 7;
		SetupStatus(Now.AddDays(-9));
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.No);

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsFalse(updated);
		_databaseServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task AutoUpdateShouldRunWithoutAsking()
	{
		_settings.Database.UpdateIntervalDays = 7;
		_settings.Database.AutoUpdate = true;
		SetupStatus(Now.AddDays(-9));

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsTrue(updated);
		_notificationServiceMock.Verify(x => x.ShowQuestionAsync(It.IsAny<string>()), Times.Never);
		_databaseServiceMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ACatalogThatWasNeverImportedShouldAskWithItsOwnQuestion()
	{
		_settings.Database.UpdateIntervalDays = 7;
		_databaseServiceMock.Setup(x => x.GetCatalogStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync([new CatalogStatusResponse { Kind = CatalogKind.Channel }]);
		_notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>())).ReturnsAsync(NotificationResult.Yes);

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsTrue(updated);
		_notificationServiceMock.Verify(x => x.ShowQuestionAsync(Resources.DatabaseSyncNeverQuestion), Times.Once);
	}

	[TestMethod]
	public async Task AFailureShouldBeReportedInsteadOfThrown()
	{
		_settings.Database.UpdateIntervalDays = 7;
		_settings.Database.AutoUpdate = true;
		SetupStatus(Now.AddDays(-9));
		_databaseServiceMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("no connection"));

		bool updated = await _sut.RunStartupCheckAsync(TestContext.CancellationToken).ConfigureAwait(false);

		Assert.IsFalse(updated);
		_eventServiceMock.Verify(
			x => x.Publish(It.Is<ErrorOccuredEvent>(@event => @event.Message == Resources.DatabaseSyncFailed)),
			Times.Once);
	}

	/// <summary>
	/// Says that every list was read at the given time.
	/// </summary>
	private void SetupStatus(DateTime lastChecked)
		=> _databaseServiceMock.Setup(x => x.GetCatalogStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(
			[
				.. Enum.GetValues<CatalogKind>().Select(kind => new CatalogStatusResponse
				{
					Kind = kind,
					FirstImported = lastChecked,
					LastChecked = lastChecked,
					LastChanged = lastChecked
				})
			]);

	public TestContext TestContext { get; set; } = default!;
}
