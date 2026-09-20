// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Tests.Persistence;

/// <summary>
/// Stores user defined channels in a real SQLite database through <see cref="ICustomChannelService"/>.
/// </summary>
[TestClass]
public sealed class CustomChannelPersistenceTests
{
	private SqliteTestDatabase _database = default!;
	private ICustomChannelService _sut = default!;

	[TestInitialize]
	public async Task InitializeAsync()
	{
		_database = SqliteTestDatabase.InMemory();
		await _database.WithRepositoryAsync(r => r.MigrateDatabaseAsync()).ConfigureAwait(false);
		_sut = _database.Services.GetRequiredService<ICustomChannelService>();
	}

	[TestCleanup]
	public void Cleanup()
		=> _database.Dispose();

	[TestMethod]
	public async Task CreateAsyncShouldStoreTheChannelWithAnyUrlScheme()
	{
		int id = await _sut
			.CreateAsync(CreateChannel("Local camera", "rtsp://192.168.12.1:554"))
			.ConfigureAwait(false);

		IReadOnlyList<CustomChannelResponse> channels = await _sut.GetChannelsAsync().ConfigureAwait(false);

		Assert.IsGreaterThan(0, id);
		Assert.HasCount(1, channels);
		Assert.AreEqual("Local camera", channels[0].Name);
		Assert.AreEqual("rtsp://192.168.12.1:554", channels[0].Url);
		Assert.AreEqual("Local", channels[0].GroupTitle);
	}

	[TestMethod]
	public async Task GetChannelsAsyncShouldOrderByName()
	{
		_ = await _sut.CreateAsync(CreateChannel("Zebra", "udp://239.0.0.1:1234")).ConfigureAwait(false);
		_ = await _sut.CreateAsync(CreateChannel("Alpha", "http://alpha.example")).ConfigureAwait(false);

		IReadOnlyList<CustomChannelResponse> channels = await _sut.GetChannelsAsync().ConfigureAwait(false);

		Assert.AreEqual("Alpha", channels[0].Name);
		Assert.AreEqual("Zebra", channels[1].Name);
	}

	[TestMethod]
	public async Task UpdateAsyncShouldReplaceTheStoredValues()
	{
		int id = await _sut.CreateAsync(CreateChannel("Camera", "rtsp://192.168.12.1:554")).ConfigureAwait(false);

		bool updated = await _sut
			.UpdateAsync(new CustomChannelResponse { Id = id, Name = "Garden", Url = "rtsp://192.168.12.2:554", TvgId = "garden" })
			.ConfigureAwait(false);

		IReadOnlyList<CustomChannelResponse> channels = await _sut.GetChannelsAsync().ConfigureAwait(false);

		Assert.IsTrue(updated);
		Assert.HasCount(1, channels);
		Assert.AreEqual("Garden", channels[0].Name);
		Assert.AreEqual("rtsp://192.168.12.2:554", channels[0].Url);
		Assert.AreEqual("garden", channels[0].TvgId);
		Assert.IsNull(channels[0].GroupTitle);
	}

	[TestMethod]
	public async Task UpdateAsyncShouldReportAnUnknownChannel()
	{
		bool updated = await _sut
			.UpdateAsync(new CustomChannelResponse { Id = 42, Name = "Ghost", Url = "http://ghost.example" })
			.ConfigureAwait(false);

		Assert.IsFalse(updated);
	}

	[TestMethod]
	public async Task DeleteAsyncShouldRemoveTheChannel()
	{
		int id = await _sut.CreateAsync(CreateChannel("Camera", "rtsp://192.168.12.1:554")).ConfigureAwait(false);

		bool deleted = await _sut.DeleteAsync(id).ConfigureAwait(false);

		Assert.IsTrue(deleted);
		Assert.IsEmpty(await _sut.GetChannelsAsync().ConfigureAwait(false));
	}

	[TestMethod]
	public async Task ResetCatalogAsyncShouldKeepTheCustomChannels()
	{
		_ = await _sut.CreateAsync(CreateChannel("Camera", "rtsp://192.168.12.1:554")).ConfigureAwait(false);
		await _database.WithRepositoryAsync(async repository =>
		{
			await repository.Channels.CreateAsync(new ChannelEntity
			{
				Channel = "DasErste.de",
				Name = "Das Erste",
				Country = "DE",
				AltNames = [],
				Categories = [],
				Owners = []
			}).ConfigureAwait(false);

			_ = await repository.CommitChangesAsync().ConfigureAwait(false);
		}).ConfigureAwait(false);

		int deleted = await _database
			.WithRepositoryAsync(r => r.ResetCatalogAsync())
			.ConfigureAwait(false);

		Assert.AreEqual(1, deleted);
		Assert.HasCount(1, await _sut.GetChannelsAsync().ConfigureAwait(false));
	}

	private static CustomChannelResponse CreateChannel(string name, string url) => new()
	{
		Name = name,
		Url = url,
		GroupTitle = "Local"
	};
}
