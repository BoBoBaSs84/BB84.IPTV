// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents the service that merges stored playlists into a new one.
/// </summary>
/// <remarks>
/// The sources are loaded as copies and the result is a new playlist, so a merge can never change
/// a source, not even when it is merged into itself.
/// </remarks>
/// <param name="playlistService">The service that loads and stores the playlists.</param>
internal sealed class MergeService(IPlaylistService playlistService) : IMergeService
{
	public async Task<MergePreviewResponse> PreviewAsync(MergeRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		List<IPlaylist> sources = await LoadSourcesAsync(request.PlaylistIds, cancellationToken)
			.ConfigureAwait(false);

		List<EntryModel> entries = [.. sources
			.SelectMany(source => source.Entries)
			.Select(entry => Remap(entry, request.GroupMappings))];

		int sourceEntryCount = entries.Count;
		List<EntryModel> merged = RemoveDuplicates(entries, request.DuplicateMode, request.DuplicateResolution);

		// The header of the first source is kept, the others have no place to go.
		PlaylistModel result = sources.Count > 0
			? new PlaylistModel(sources[0], merged)
			: new PlaylistModel(new PlaylistModel(), merged);

		return new MergePreviewResponse
		{
			Playlist = result,
			SourceCount = sources.Count,
			SourceEntryCount = sourceEntryCount,
			DuplicateCount = sourceEntryCount - merged.Count,
			Groups = [.. result.Entries
				.Select(entry => entry.Metadata.GroupTitle ?? string.Empty)
				.Distinct(StringComparer.CurrentCultureIgnoreCase)
				.Order(StringComparer.CurrentCultureIgnoreCase)]
		};
	}

	public async Task<int> MergeAsync(MergeRequest request, CancellationToken cancellationToken = default)
	{
		MergePreviewResponse preview = await PreviewAsync(request, cancellationToken)
			.ConfigureAwait(false);

		if (preview.SourceCount is 0)
			throw new InvalidOperationException(Resources.MergeWithoutSources);

		string name = string.IsNullOrWhiteSpace(request.Name)
			? Resources.MergedPlaylistName
			: request.Name.Trim();

		return await playlistService
			.CreateAsync(name, preview.Playlist, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <summary>
	/// Loads the sources in the requested order, a playlist that no longer exists is skipped.
	/// </summary>
	private async Task<List<IPlaylist>> LoadSourcesAsync(IReadOnlyList<int> playlistIds, CancellationToken cancellationToken)
	{
		List<IPlaylist> sources = [];

		foreach (int id in playlistIds)
		{
			IPlaylist? playlist = await playlistService
				.LoadAsync(id, cancellationToken)
				.ConfigureAwait(false);

			if (playlist is not null)
				sources.Add(playlist);
		}

		return sources;
	}

	/// <summary>
	/// Copies the entry and renames its group title if the mappings hold the old title.
	/// </summary>
	private static EntryModel Remap(EntryModel entry, IReadOnlyDictionary<string, string>? groupMappings)
	{
		EntryModel copy = new(entry);

		if (groupMappings is null || groupMappings.Count is 0)
			return copy;

		if (groupMappings.TryGetValue(copy.Metadata.GroupTitle ?? string.Empty, out string? target))
			copy.Metadata.GroupTitle = string.IsNullOrWhiteSpace(target) ? null : target;

		return copy;
	}

	/// <summary>
	/// Drops the entries that share a key with another entry, an entry without a key is always kept.
	/// </summary>
	private static List<EntryModel> RemoveDuplicates(List<EntryModel> entries, MergeDuplicateMode mode, MergeDuplicateResolution resolution)
	{
		if (mode is MergeDuplicateMode.None)
			return entries;

		// Every key points to the entry that currently wins it, so a later entry finds the winner
		// by any of its keys; the winner keeps its own position.
		Dictionary<string, int> winnerByKey = new(StringComparer.CurrentCultureIgnoreCase);
		bool[] dropped = new bool[entries.Count];

		for (int index = 0; index < entries.Count; index++)
		{
			string[] keys = [.. GetKeys(entries[index], mode)];

			if (keys.Length is 0)
				continue;

			int[] matches = [.. keys
				.Where(winnerByKey.ContainsKey)
				.Select(key => winnerByKey[key])
				.Distinct()];

			if (matches.Length is 0)
			{
				Point(winnerByKey, keys, index);
				continue;
			}

			if (resolution is MergeDuplicateResolution.KeepFirst)
			{
				dropped[index] = true;
				Point(winnerByKey, keys, matches.Min());
				continue;
			}

			foreach (int match in matches)
			{
				dropped[match] = true;
				Point(winnerByKey, [.. GetKeys(entries[match], mode)], index);
			}

			Point(winnerByKey, keys, index);
		}

		return [.. entries.Where((_, index) => !dropped[index])];
	}

	/// <summary>
	/// Lets every key point to the entry that wins it.
	/// </summary>
	private static void Point(Dictionary<string, int> winnerByKey, IReadOnlyList<string> keys, int index)
	{
		foreach (string key in keys)
			winnerByKey[key] = index;
	}

	/// <summary>
	/// Gets the values that identify the entry, an empty value never identifies anything.
	/// </summary>
	private static IEnumerable<string> GetKeys(EntryModel entry, MergeDuplicateMode mode)
	{
		if (mode is MergeDuplicateMode.ByTvgId or MergeDuplicateMode.ByTvgIdOrUrl && !string.IsNullOrWhiteSpace(entry.Metadata.TvgId))
			yield return $"tvg-id:{entry.Metadata.TvgId.Trim()}";

		if (mode is MergeDuplicateMode.ByUrl or MergeDuplicateMode.ByTvgIdOrUrl && !string.IsNullOrWhiteSpace(entry.FilePath))
			yield return $"url:{entry.FilePath.Trim()}";
	}
}