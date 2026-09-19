using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Providers;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;
using BB84.IPTV.M3U.Editor.Infrastructure.Services;

using Microsoft.Extensions.Logging;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Services;

[TestClass]
public sealed class SettingsServiceTests
{
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly Mock<ILoggerService<SettingsService>> _loggerServiceMock = new();
	private readonly Mock<IProviderService> _providerServiceMock = new();
	private readonly Mock<IDirectoryProvider> _directoryProviderMock = new();
	private readonly Mock<IFileProvider> _fileProviderMock = new();
	private readonly ApplicationSettings _applicationSettings = new();
	private readonly SettingsService _sut;

	public SettingsServiceTests()
	{
		_providerServiceMock.SetupGet(x => x.Directory)
			.Returns(_directoryProviderMock.Object);
		_providerServiceMock.SetupGet(x => x.File)
			.Returns(_fileProviderMock.Object);

		_sut = new SettingsService(_eventServiceMock.Object, _loggerServiceMock.Object, _providerServiceMock.Object, _applicationSettings);
	}

	[TestMethod]
	public void ChangingLanguageShouldPublishLanguageChangedEvent()
	{
		_applicationSettings.General.Language = Language.German;

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LanguageChangedEvent>()), Times.Once);
	}

	[TestMethod]
	public void ChangingOtherSettingsShouldNotPublishLanguageChangedEvent()
	{
		_applicationSettings.General.AutoSave = false;

		_eventServiceMock.Verify(x => x.Publish(It.IsAny<LanguageChangedEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task LoadAsyncShouldCreateDefaultSettingsWhenFileDoesNotExist()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		string fileContent = ApplicationSettings.Write(new ApplicationSettings());

		_fileProviderMock.Setup(x => x.Exists(It.IsAny<string>()))
			.Returns(false);
		_fileProviderMock.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		_fileProviderMock.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(fileContent);

		await _sut.LoadAsync(cancellationToken)
			.ConfigureAwait(false);

		_fileProviderMock.Verify(x => x.WriteAllTextAsync(ApplicationPaths.SettingsFilePath, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
		_fileProviderMock.Verify(x => x.ReadAllTextAsync(ApplicationPaths.SettingsFilePath, It.IsAny<CancellationToken>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<SettingsSavedEvent>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<SettingsLoadedEvent>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task LoadAsyncShouldLoadSettingsWhenFileExists()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		ApplicationSettings expectedSettings = new();
		expectedSettings.General.AutoSave = false;
		expectedSettings.General.AutoSaveInterval = 30;
		expectedSettings.General.Language = Language.German;
		string fileContent = ApplicationSettings.Write(expectedSettings);

		_fileProviderMock.Setup(x => x.Exists(It.IsAny<string>()))
			.Returns(true);
		_fileProviderMock.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(fileContent);

		await _sut.LoadAsync(cancellationToken)
			.ConfigureAwait(false);

		Assert.AreEqual(expectedSettings.General.AutoSave, _applicationSettings.General.AutoSave);
		Assert.AreEqual(expectedSettings.General.AutoSaveInterval, _applicationSettings.General.AutoSaveInterval);
		Assert.AreEqual(expectedSettings.General.Language, _applicationSettings.General.Language);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<SettingsLoadedEvent>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<SettingsSavedEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task LoadAsyncShouldLogAndPublishErrorOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;

		_fileProviderMock.Setup(x => x.Exists(It.IsAny<string>()))
			.Throws<Exception>();

		await _sut.LoadAsync(cancellationToken)
			.ConfigureAwait(false);

		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}

	[TestMethod]
	public async Task SaveAsyncShouldWriteSettingsAndPublishEvent()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		ApplicationSettings settings = new();
		settings.General.Language = Language.German;
		string expectedFileContent = ApplicationSettings.Write(settings);

		_fileProviderMock.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		await _sut.SaveAsync(settings, cancellationToken)
			.ConfigureAwait(false);

		_directoryProviderMock.Verify(x => x.CreateDirectory(ApplicationPaths.DataDirectory), Times.Once);
		_fileProviderMock.Verify(x => x.WriteAllTextAsync(ApplicationPaths.SettingsFilePath, expectedFileContent, It.IsAny<CancellationToken>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<SettingsSavedEvent>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Never);
	}

	[TestMethod]
	public async Task SaveAsyncShouldLogAndPublishErrorOnException()
	{
		CancellationToken cancellationToken = CancellationToken.None;
		ApplicationSettings settings = new();

		_fileProviderMock.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new IOException());

		await _sut.SaveAsync(settings, cancellationToken)
			.ConfigureAwait(false);

		_loggerServiceMock.Verify(x => x.Log(It.IsAny<Action<ILogger, string, Exception?>>(), It.IsAny<string>(), It.IsAny<Exception?>()), Times.Once);
		_eventServiceMock.Verify(x => x.Publish(It.IsAny<ErrorOccuredEvent>()), Times.Once);
	}
}
