using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Application.ViewModels;

using Moq;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class DatabaseViewModelTests : IDisposable
{
	private readonly Mock<IDatabaseService> _databaseServiceMock = new();
	private readonly Mock<ILogoService> _logoServiceMock = new();
	private readonly Mock<IEventService> _eventServiceMock = new();
	private readonly DatabaseViewModel _sut;

	public DatabaseViewModelTests()
	{
		_logoServiceMock.Setup(x => x.GetStatusAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new LogoCacheStatusResponse { TotalCount = 10, CachedCount = 4, CachedBytes = 2048 });

		_sut = new DatabaseViewModel(_eventServiceMock.Object, _databaseServiceMock.Object, _logoServiceMock.Object, new ApplicationSettings());
	}

	/// <summary>
	/// The view model stops a running logo download when it is disposed.
	/// </summary>
	public void Dispose()
		=> _sut.Dispose();

	[TestMethod]
	public async Task CreatingTheDatabaseShouldEnableCheckAndImport()
	{
		_databaseServiceMock.Setup(x => x.CreateDatabaseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
		int checkChanged = 0;
		int importChanged = 0;
		_sut.CheckDatabaseCommand.CanExecuteChanged += (s, e) => checkChanged++;
		_sut.ImportDatabaseCommand.CanExecuteChanged += (s, e) => importChanged++;

		Assert.IsFalse(_sut.CheckDatabaseCommand.CanExecute());
		Assert.IsFalse(_sut.ImportDatabaseCommand.CanExecute());

		await _sut.CreateDatabaseCommand.ExecuteAsync().ConfigureAwait(false);

		Assert.IsTrue(_sut.DatabaseCreated);
		Assert.IsTrue(_sut.CheckDatabaseCommand.CanExecute());
		Assert.IsTrue(_sut.ImportDatabaseCommand.CanExecute());
		Assert.IsGreaterThan(0, checkChanged);
		Assert.IsGreaterThan(0, importChanged);
	}
}