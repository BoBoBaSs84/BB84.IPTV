using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class MainViewModelTests
{
	[TestMethod]
	public void ConstructorShouldSetPropertiesCorrect()
	{
		Mock<IEventService> eventServiceMock = new();
		Mock<IHostEnvironment> hostEnvironmentMock = new();
		hostEnvironmentMock.Setup(x => x.ApplicationName).Returns("TestApp");
		hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("TestEnv");
		Mock<INotificationService> notificationServiceMock = new();
		Mock<IUserService> userServiceMock = new();
		userServiceMock.Setup(x => x.Domain).Returns("TestDomain");
		userServiceMock.Setup(x => x.Name).Returns("TestUser");
		userServiceMock.Setup(x => x.Machine).Returns("TestMachine");
		Mock<INavigationService> navigationServiceMock = new();

		MainViewModel viewModel = new(eventServiceMock.Object, hostEnvironmentMock.Object, notificationServiceMock.Object, userServiceMock.Object, navigationServiceMock.Object);

		Assert.AreEqual("TestApp - TestEnv", viewModel.ApplicationTitle);
		Assert.AreEqual("TestDomain\\TestUser@TestMachine", viewModel.CurrentUser);
	}

	[TestMethod]
	[DataRow(NotificationResult.Yes, 1)]
	[DataRow(NotificationResult.No, 0)]
	public async Task ExitApplicationCommandShouldPublishExitRequestedOnlyWhenConfirmed(NotificationResult answer, int expectedPublishCount)
	{
		Mock<IEventService> eventServiceMock = new();
		Mock<INotificationService> notificationServiceMock = new();
		notificationServiceMock.Setup(x => x.ShowQuestionAsync(It.IsAny<string>()))
			.ReturnsAsync(answer);

		MainViewModel viewModel = new(eventServiceMock.Object, new Mock<IHostEnvironment>().Object, notificationServiceMock.Object, new Mock<IUserService>().Object, new Mock<INavigationService>().Object);

		await viewModel.ExitApplicationCommand.ExecuteAsync().ConfigureAwait(false);

		eventServiceMock.Verify(x => x.Publish(It.IsAny<ExitRequestedEvent>()), Times.Exactly(expectedPublishCount));
	}

	[TestMethod]
	public void TheStatusBarShouldFollowBothStatusEvents()
	{
		EventService eventService = new(NullLogger<EventService>.Instance);
		MainViewModel viewModel = CreateMainViewModel(eventService);

		// A status without auto clear stays, so a long running operation can keep it.
		eventService.Publish(new StatusChangedEvent("Working"));
		Assert.AreEqual("Working", viewModel.StatusText);

		eventService.Publish(new DelayedStatusChangedEvent("Done", 10000));
		Assert.AreEqual("Done", viewModel.StatusText);
	}

	[TestMethod]
	public void TheProgressBarShouldFollowTheProgressEvent()
	{
		EventService eventService = new(NullLogger<EventService>.Instance);
		MainViewModel viewModel = CreateMainViewModel(eventService);

		eventService.Publish(new ProgressChangedEvent(25));

		Assert.AreEqual(25, viewModel.ProgressBarValue);
		Assert.AreEqual(100, viewModel.ProgressBarMaximum);
		Assert.IsTrue(viewModel.ProgressBarVisible);

		// A finished operation hides the bar again.
		eventService.Publish(new ProgressChangedEvent(100));

		Assert.IsFalse(viewModel.ProgressBarVisible);
	}

	private static MainViewModel CreateMainViewModel(IEventService eventService)
		=> new(
			eventService,
			new Mock<IHostEnvironment>().Object,
			new Mock<INotificationService>().Object,
			new Mock<IUserService>().Object,
			new Mock<INavigationService>().Object);
}
