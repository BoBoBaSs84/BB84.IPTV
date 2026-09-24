// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Windows.Input;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class SettingsViewModelTests
{
	private readonly Mock<ISettingsService> _settingsServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly Mock<IFileDialogService> _fileDialogServiceMock = new();
	private readonly Mock<IPathService> _pathServiceMock = new();
	private readonly ApplicationSettings _applicationSettings = new();
	private readonly SettingsViewModel _sut;

	public SettingsViewModelTests()
	{
		_pathServiceMock.SetupGet(x => x.DataDirectory).Returns(Path.Combine(Path.GetTempPath(), "data"));
		_pathServiceMock.SetupGet(x => x.LogoDirectory).Returns(Path.Combine(Path.GetTempPath(), "logos"));
		_pathServiceMock.SetupGet(x => x.LogDirectory).Returns(Path.Combine(Path.GetTempPath(), "logs"));
		_pathServiceMock.Setup(x => x.IsValidDirectory(It.IsAny<string>())).Returns(true);

		_sut = new SettingsViewModel(
			_settingsServiceMock.Object,
			_eventServiceMock.Object,
			_fileDialogServiceMock.Object,
			_pathServiceMock.Object,
			_applicationSettings);
	}

	[TestMethod]
	public void TheSectionsShouldBeTheOnesOfTheApplicationSettings()
	{
		Assert.AreSame(_applicationSettings.General, _sut.General);
		Assert.AreSame(_applicationSettings.Database, _sut.Database);
		Assert.AreSame(_applicationSettings.Paths, _sut.Paths);
		Assert.AreSame(_applicationSettings.Logo, _sut.Logo);
	}

	[TestMethod]
	public void TheParallelDownloadLimitShouldBeTheOneOfTheRequest()
		=> Assert.AreEqual(LogoCacheRequest.MaxParallelLimit, SettingsViewModel.MaxParallelDownloadsLimit);

	[TestMethod]
	public void ThePathsInUseShouldComeFromThePathService()
	{
		Assert.AreEqual(_pathServiceMock.Object.DataDirectory, _sut.DataDirectoryInUse);
		Assert.AreEqual(_pathServiceMock.Object.LogoDirectory, _sut.LogoDirectoryInUse);
		Assert.AreEqual(_pathServiceMock.Object.LogDirectory, _sut.LogDirectoryInUse);
	}

	[TestMethod]
	public async Task SaveShouldStoreTheSettings()
	{
		await _sut.SaveCommand.ExecuteAsync().ConfigureAwait(false);

		_settingsServiceMock.Verify(x => x.SaveAsync(_applicationSettings, It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task SaveShouldAskForARestartWhenAPathWasChanged()
	{
		_pathServiceMock.Setup(x => x.HasPendingChanges()).Returns(true);

		await _sut.SaveCommand.ExecuteAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<DataPathsChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task SaveShouldNotAskForARestartWhenThePathsStayed()
	{
		_pathServiceMock.Setup(x => x.HasPendingChanges()).Returns(false);

		await _sut.SaveCommand.ExecuteAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<DataPathsChangedEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task SaveShouldWarnAndKeepTheFileWhenAPathCannotBeUsed()
	{
		_sut.Paths.DataDirectory = "relative/directory";
		_pathServiceMock.Setup(x => x.IsValidDirectory("relative/directory")).Returns(false);

		await _sut.SaveCommand.ExecuteAsync().ConfigureAwait(false);

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<WarningOccuredEvent>()), Times.Once);
		_settingsServiceMock.Verify(x => x.SaveAsync(It.IsAny<ApplicationSettings>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task BrowseShouldTakeThePickedDirectory()
	{
		string picked = Path.Combine(Path.GetTempPath(), "picked");
		_fileDialogServiceMock.Setup(x => x.ShowOpenFolderDialogAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync(picked);

		await _sut.BrowseDataDirectoryCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.BrowseLogoDirectoryCommand.ExecuteAsync().ConfigureAwait(false);
		await _sut.BrowseLogDirectoryCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(picked, _sut.Paths.DataDirectory);
		Assert.AreEqual(picked, _sut.Paths.LogoDirectory);
		Assert.AreEqual(picked, _sut.Paths.LogDirectory);
	}

	[TestMethod]
	public async Task BrowseShouldKeepThePathWhenTheDialogWasCancelled()
	{
		string configured = Path.Combine(Path.GetTempPath(), "configured");
		_sut.Paths.DataDirectory = configured;
		_fileDialogServiceMock.Setup(x => x.ShowOpenFolderDialogAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync((string?)null);

		await _sut.BrowseDataDirectoryCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.AreEqual(configured, _sut.Paths.DataDirectory);
	}

	[TestMethod]
	public async Task BrowseShouldStartInTheConfiguredDirectoryOrTheOneInUse()
	{
		string configured = Path.Combine(Path.GetTempPath(), "configured");
		_fileDialogServiceMock.Setup(x => x.ShowOpenFolderDialogAsync(It.IsAny<string>(), It.IsAny<string?>()))
			.ReturnsAsync((string?)null);

		await _sut.BrowseDataDirectoryCommand.ExecuteAsync().ConfigureAwait(false);
		_fileDialogServiceMock.Verify(x => x.ShowOpenFolderDialogAsync(It.IsAny<string>(), _sut.DataDirectoryInUse), Times.Once);

		_sut.Paths.DataDirectory = configured;
		await _sut.BrowseDataDirectoryCommand.ExecuteAsync().ConfigureAwait(false);
		_fileDialogServiceMock.Verify(x => x.ShowOpenFolderDialogAsync(It.IsAny<string>(), configured), Times.Once);
	}

	[TestMethod]
	public async Task AFailedSaveShouldPublishALocalizedError()
	{
		TaskCompletionSource<ErrorOccuredEvent> published = new();
		_eventServiceMock.Setup(x => x.Publish(It.IsAny<ErrorOccuredEvent>()))
			.Callback((ErrorOccuredEvent @event) => published.TrySetResult(@event));
		_settingsServiceMock.Setup(x => x.SaveAsync(It.IsAny<ApplicationSettings>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new IOException("no disk"));

		// The UI runs the command, which reports a failure to its error handler instead of throwing.
		((ICommand)_sut.SaveCommand).Execute(null);

		ErrorOccuredEvent reported = await published.Task
			.WaitAsync(TimeSpan.FromSeconds(5))
			.ConfigureAwait(false);

		Assert.AreEqual(Resources.SettingsOperationFailed, reported.Message);
		Assert.IsNotNull(reported.Exception);
	}

	[TestMethod]
	public async Task LoadShouldReadTheSettings()
	{
		await _sut.LoadCommand.ExecuteAsync().ConfigureAwait(false);

		_settingsServiceMock.Verify(x => x.LoadAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}