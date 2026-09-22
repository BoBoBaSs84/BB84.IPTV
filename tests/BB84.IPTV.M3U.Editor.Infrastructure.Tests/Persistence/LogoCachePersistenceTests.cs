// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Settings;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Entities;
using BB84.IPTV.M3U.Editor.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Caches logos of a real SQLite catalog and exports a playlist that points at the cached files.
/// </summary>
[TestClass]
public sealed class LogoCachePersistenceTests
{
	private const string LogoUrl = "https://logo.example/ard.png";

	private SqliteTestDatabase _database = default!;
	private ILogoService _sut = default!;
	private IPlaylistService _playlistService = default!;
	private ApplicationSettings _settings = default!;
	private string _directory = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_directory = Directory.CreateTempSubdirectory().FullName;
		_database = SqliteTestDatabase.InMemory(services =>
		{
			services.AddSingleton(CreateDownloadService());
			services.AddSingleton<ILogoStoreService>(new TestLogoStoreService(_directory));
		});

		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);

		_sut = _database.Services.GetRequiredService<ILogoService>();
		_playlistService = _database.Services.GetRequiredService<IPlaylistService>();
		_settings = _database.Services.GetRequiredService<ApplicationSettings>();

		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Logos.CreateAsync(new LogoEntity
			{
				Channel = "DasErste.de",
				Url = LogoUrl,
				Format = "PNG",
				Tags = [],
				Width = 512,
				Height = 512
			}).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	[TestCleanup]
	public void Cleanup()
	{
		_database.Dispose();
		Directory.Delete(_directory, true);
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldWriteTheFileAndRememberIt()
	{
		int downloaded = await _sut.CacheLogosAsync().ConfigureAwait(false);

		LogoCacheStatusResponse status = await _sut.GetStatusAsync().ConfigureAwait(false);
		string? path = await _sut.GetLocalPathAsync("DasErste.de").ConfigureAwait(false);

		Assert.AreEqual(1, downloaded);
		Assert.AreEqual(1, status.CachedCount);
		Assert.AreEqual(0, status.MissingCount);
		Assert.IsNotNull(path);
		Assert.IsTrue(File.Exists(path));
	}

	[TestMethod]
	public async Task CacheLogosAsyncShouldBeResumable()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);

		int again = await _sut.CacheLogosAsync().ConfigureAwait(false);

		Assert.AreEqual(0, again, "Nothing is missing, so nothing is downloaded again.");
	}

	[TestMethod]
	public async Task ExportShouldWriteTheCachedFileAsTheLogo()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		int id = await CreatePlaylistAsync().ConfigureAwait(false);
		string exportPath = Path.Combine(_directory, "export.m3u");

		_ = await _playlistService.ExportAsync(id, exportPath).ConfigureAwait(false);

		string content = await File.ReadAllTextAsync(exportPath).ConfigureAwait(false);
		string? cachedPath = await _sut.GetLocalPathAsync("DasErste.de").ConfigureAwait(false);
		IPlaylist stored = (await _playlistService.LoadAsync(id).ConfigureAwait(false))!;

		Assert.Contains(cachedPath!, content);
		Assert.DoesNotContain(LogoUrl, content);
		Assert.AreEqual(LogoUrl, stored.Entries.First().Metadata.TvgLogo, "The stored playlist keeps the URL.");
	}

	[TestMethod]
	public async Task ExportShouldWriteARelativePathWhenAsked()
	{
		_settings.Logo.ExportPathStyle = Application.Enumerators.LogoPathStyle.Relative;
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		int id = await CreatePlaylistAsync().ConfigureAwait(false);
		string exportPath = Path.Combine(_directory, "export.m3u");

		_ = await _playlistService.ExportAsync(id, exportPath).ConfigureAwait(false);

		string content = await File.ReadAllTextAsync(exportPath).ConfigureAwait(false);
		string? cachedPath = await _sut.GetLocalPathAsync("DasErste.de").ConfigureAwait(false);

		Assert.Contains(Path.GetRelativePath(_directory, cachedPath!), content);
		Assert.DoesNotContain(_directory, content);
	}

	[TestMethod]
	public async Task ExportShouldKeepTheUrlWhenLocalPathsAreOff()
	{
		_settings.Logo.UseLocalPathsOnExport = false;
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);
		int id = await CreatePlaylistAsync().ConfigureAwait(false);
		string exportPath = Path.Combine(_directory, "export.m3u");

		_ = await _playlistService.ExportAsync(id, exportPath).ConfigureAwait(false);

		Assert.Contains(LogoUrl, await File.ReadAllTextAsync(exportPath).ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ClearAsyncShouldDeleteTheFilesAndForgetThem()
	{
		_ = await _sut.CacheLogosAsync().ConfigureAwait(false);

		int deleted = await _sut.ClearAsync().ConfigureAwait(false);

		LogoCacheStatusResponse status = await _sut.GetStatusAsync().ConfigureAwait(false);

		Assert.AreEqual(1, deleted);
		Assert.AreEqual(0, status.CachedCount);
		Assert.IsNull(await _sut.GetLocalPathAsync("DasErste.de").ConfigureAwait(false));
	}

	private async Task<int> CreatePlaylistAsync()
		=> await _playlistService.CreateAsync("Mine", new PlaylistModel(new PlaylistModel(),
		[
			new EntryModel("Das Erste", "https://example.com/ard.m3u8", metadata: new MetadataModel { TvgId = "DasErste.de", TvgLogo = LogoUrl })
		])).ConfigureAwait(false);

	private static IDownloadService CreateDownloadService()
	{
		Mock<IDownloadService> downloadServiceMock = new();
		downloadServiceMock.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new LogoDownloadResponse { Content = [0x89, 0x50, 0x4E, 0x47], ETag = "\"tag\"", ContentType = "image/png" });

		return downloadServiceMock.Object;
	}

	/// <summary>
	/// Keeps the cached files in a temporary directory instead of the application data directory.
	/// </summary>
	private sealed class TestLogoStoreService(string root) : ILogoStoreService
	{
		public async Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default)
		{
			string directory = Path.Combine(root, "logos", channel);
			_ = Directory.CreateDirectory(directory);
			string path = Path.Combine(directory, fileName);
			await File.WriteAllBytesAsync(path, content, cancellationToken).ConfigureAwait(false);

			return path;
		}

		public bool Exists(string? path)
			=> !string.IsNullOrWhiteSpace(path) && File.Exists(path);

		public int Clear()
		{
			string directory = Path.Combine(root, "logos");

			if (!Directory.Exists(directory))
				return 0;

			int deleted = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length;
			Directory.Delete(directory, true);

			return deleted;
		}
	}
}