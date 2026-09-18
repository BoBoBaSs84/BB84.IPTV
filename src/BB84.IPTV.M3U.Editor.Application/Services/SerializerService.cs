// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;
using System.Text;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents a serializer for M3U playlists, providing methods to serialize and deserialize playlist data.
/// </summary>
public sealed class SerializerService : ISerializerService
{
	private static readonly string[] Separator = ["\r\n", "\n"];

	/// <inheritdoc/>
	public IPlaylist Deserialize(string filePath)
	{
		byte[] fileContent = File.ReadAllBytes(filePath);
		return Deserialize(fileContent);
	}

	/// <inheritdoc/>
	public IPlaylist Deserialize(string[] fileLines)
	{
		PlaylistModel playlist = new();

		if (fileLines.Length == 0 || !fileLines[0].StartsWith("#EXTM3U", StringComparison.OrdinalIgnoreCase))
			throw new InvalidDataException("The file is not a valid M3U file.");

		string extM3uLine = fileLines[0];
		string[] attributes = extM3uLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
		foreach (string attribute in attributes)
		{
			if (attribute.StartsWith("url-tvg=", StringComparison.OrdinalIgnoreCase))
				playlist.UrlTvg = attribute.Split('=')[1].Trim('"');
			else if (attribute.StartsWith("refresh=", StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(attribute.Split('=')[1].Trim('"'), out int refresh))
					playlist.Refresh = refresh;
			}
			else if (attribute.StartsWith("cache=", StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(attribute.Split('=')[1].Trim('"'), out int cache))
					playlist.Cache = cache;
			}
		}

		List<EntryModel> entries = [];
		EntryModel? currentEntry = null;

		foreach (string line in fileLines.Skip(1)) // Skip the #EXTM3U line
		{
			if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
			{
				string metadataLine = line[8..];

				string[] metadataParts = metadataLine.Split(' ', '=', ',');

				int duration = default;
				if (int.TryParse(metadataParts.FirstOrDefault() ?? string.Empty, out int parsed))
					duration = parsed;

				MetadataModel metadata = new()
				{
					Censored = line.Contains("censored=\"1\"")
				};

				if (metadataLine.Contains("tvg-id"))
					metadata.TvgId = GetMetadataValue(metadataLine, "tvg-id");
				if (metadataLine.Contains("tvg-name"))
					metadata.TvgName = GetMetadataValue(metadataLine, "tvg-name");
				if (metadataLine.Contains("tvg-logo"))
					metadata.TvgLogo = GetMetadataValue(metadataLine, "tvg-logo");
				if (metadataLine.Contains("group_id"))
					metadata.GroupId = GetMetadataValue(metadataLine, "group_id");
				if (metadataLine.Contains("group-title"))
					metadata.GroupTitle = GetMetadataValue(metadataLine, "group-title");

				int titleIndex = metadataLine.LastIndexOf(',');
				string title = titleIndex >= 0 ? metadataLine[(titleIndex + 1)..].Trim() : string.Empty;

				currentEntry = new EntryModel(title, string.Empty, duration, null, metadata);
			}
			else if (line.StartsWith("#EXTGRP:", StringComparison.OrdinalIgnoreCase))
			{
				if (currentEntry is not null)
					currentEntry.Grouping = line[8..];
			}
			else if (!line.StartsWith("#", StringComparison.OrdinalIgnoreCase))
			{
				if (currentEntry is not null)
				{
					currentEntry.FilePath = line.Trim();
					entries.Add(currentEntry);
				}
				currentEntry = null;
			}
		}

		return new PlaylistModel(playlist, entries);
	}

	/// <inheritdoc/>
	public IPlaylist Deserialize(byte[] fileContent)
	{
		string contentString = Encoding.UTF8.GetString(fileContent);
		string[] fileLines = contentString.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
		return Deserialize(fileLines);
	}

	/// <inheritdoc/>
	public string Serialize(IPlaylist playlist)
	{
		StringBuilder sb = new();
		sb.Append("#EXTM3U");
		if (playlist.UrlTvg is not null)
			sb.AppendFormat(CultureInfo.InvariantCulture, " url-tvg=\"{0}\"", playlist.UrlTvg);
		if (playlist.Cache > 0)
			sb.AppendFormat(CultureInfo.InvariantCulture, " cache=\"{0}\"", playlist.Cache);
		if (playlist.Deinterlace is not Deinterlace.None)
			sb.AppendFormat(CultureInfo.InvariantCulture, " deinterlace=\"{0}\"", (int)playlist.Deinterlace);
		if (playlist.Refresh > 0)
			sb.AppendFormat(CultureInfo.InvariantCulture, " refresh=\"{0}\"", playlist.Refresh);
		sb.AppendLine();

		foreach (EntryModel entry in playlist.Entries)
		{
			if (entry.Metadata != null)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "#EXTINF:{0} ", entry.Duration);
				if (entry.Metadata.Censored)
					sb.Append("censored=\"1\" ");
				if (entry.Metadata.TvgId is not null)
					sb.AppendFormat(CultureInfo.InvariantCulture, "tvg-id=\"{0}\" ", entry.Metadata.TvgId);
				if (entry.Metadata.TvgName is not null)
					sb.AppendFormat(CultureInfo.InvariantCulture, "tvg-name=\"{0}\" ", entry.Metadata.TvgName);
				if (entry.Metadata.TvgLogo is not null)
					sb.AppendFormat(CultureInfo.InvariantCulture, "tvg-logo=\"{0}\" ", entry.Metadata.TvgLogo);
				if (entry.Metadata.GroupId is not null)
					sb.AppendFormat(CultureInfo.InvariantCulture, "group_id=\"{0}\" ", entry.Metadata.GroupId);
				if (entry.Metadata.GroupTitle is not null)
					sb.AppendFormat(CultureInfo.InvariantCulture, "group-title=\"{0}\" ", entry.Metadata.GroupTitle);
				sb.AppendFormat(CultureInfo.InvariantCulture, ",{0}", entry.Title);
			}

			if (entry.Grouping is not null)
				sb.AppendFormat(CultureInfo.InvariantCulture, "#EXTGRP:{0}", entry.Grouping);

			sb.AppendLine(entry.FilePath);
		}

		return sb.ToString();
	}

	private static string GetMetadataValue(string metadataLine, string key)
	{
		string startString = metadataLine[metadataLine.IndexOf(key, StringComparison.OrdinalIgnoreCase)..];
		int indexQuoteOne = startString.IndexOf('"');
		int indexQuoteTwo = startString.IndexOf('"', indexQuoteOne + 1);
		return startString.Substring(indexQuoteOne + 1, indexQuoteTwo - 1 - indexQuoteOne);
	}
}
