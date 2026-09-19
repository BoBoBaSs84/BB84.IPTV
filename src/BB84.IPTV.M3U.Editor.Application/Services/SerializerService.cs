// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;
using BB84.IPTV.M3U.Editor.Domain.Enumerators;
using BB84.IPTV.M3U.Editor.Domain.Models;

namespace BB84.IPTV.M3U.Editor.Application.Services;

/// <summary>
/// Represents a serializer for M3U playlists, providing methods to serialize and deserialize playlist data.
/// </summary>
/// <remarks>
/// Attributes and directives without a dedicated model property are kept in <c>AdditionalAttributes</c>
/// and <c>Directives</c>, so a playlist survives a deserialize/serialize round trip.
/// </remarks>
public sealed partial class SerializerService : ISerializerService
{
	private const string ExtM3u = "#EXTM3U";
	private const string ExtInf = "#EXTINF:";
	private const string ExtGrp = "#EXTGRP:";
	private const char ByteOrderMark = '﻿';

	/// <inheritdoc/>
	public IPlaylist Deserialize(string filePath)
	{
		byte[] fileContent = File.ReadAllBytes(filePath);
		return Deserialize(fileContent);
	}

	/// <inheritdoc/>
	public IPlaylist Deserialize(string[] fileLines)
	{
		int headerIndex = Array.FindIndex(fileLines, line => !string.IsNullOrWhiteSpace(line));
		string header = headerIndex >= 0 ? fileLines[headerIndex].TrimStart(ByteOrderMark).Trim() : string.Empty;

		if (!header.StartsWith(ExtM3u, StringComparison.OrdinalIgnoreCase))
			throw new InvalidDataException("The file is not a valid M3U file.");

		PlaylistModel playlist = new();
		ReadPlaylistAttributes(playlist, header[ExtM3u.Length..]);

		List<EntryModel> entries = [];
		EntryModel? currentEntry = null;

		foreach (string rawLine in fileLines.Skip(headerIndex + 1))
		{
			string line = rawLine.Trim();

			if (line.Length == 0)
				continue;

			if (line.StartsWith(ExtInf, StringComparison.OrdinalIgnoreCase))
			{
				currentEntry = ReadEntry(line[ExtInf.Length..]);
			}
			else if (line.StartsWith(ExtGrp, StringComparison.OrdinalIgnoreCase))
			{
				if (currentEntry is not null)
					currentEntry.Grouping = line[ExtGrp.Length..];
			}
			else if (line.StartsWith('#'))
			{
				if (currentEntry is not null)
					currentEntry.Directives = currentEntry.Directives is null ? line : $"{currentEntry.Directives}\n{line}";
			}
			else
			{
				// A link without #EXTINF is a plain M3U entry, it is kept with an empty title.
				currentEntry ??= new EntryModel(string.Empty, string.Empty);
				currentEntry.FilePath = line;
				entries.Add(currentEntry);
				currentEntry = null;
			}
		}

		return new PlaylistModel(playlist, entries);
	}

	/// <inheritdoc/>
	public IPlaylist Deserialize(byte[] fileContent)
	{
		string content = Encoding.UTF8.GetString(fileContent);
		List<string> fileLines = [];

		// StringReader splits on "\r\n", "\n" and "\r".
		using StringReader reader = new(content);
		while (reader.ReadLine() is { } line)
			fileLines.Add(line);

		return Deserialize([.. fileLines]);
	}

	/// <inheritdoc/>
	public string Serialize(IPlaylist playlist)
	{
		StringBuilder sb = new(ExtM3u);

		AppendAttribute(sb, "url-tvg", playlist.UrlTvg);
		if (playlist.Cache > 0)
			AppendAttribute(sb, "cache", playlist.Cache.ToString(CultureInfo.InvariantCulture));
		if (playlist.Deinterlace is not Deinterlace.None)
			AppendAttribute(sb, "deinterlace", ((int)playlist.Deinterlace).ToString(CultureInfo.InvariantCulture));
		if (playlist.Refresh > 0)
			AppendAttribute(sb, "refresh", playlist.Refresh.ToString(CultureInfo.InvariantCulture));
		AppendRaw(sb, playlist.AdditionalAttributes);
		sb.Append('\n');

		foreach (EntryModel entry in playlist.Entries)
		{
			IMetadata metadata = entry.Metadata;

			sb.Append(ExtInf).Append(entry.Duration.ToString(CultureInfo.InvariantCulture));
			if (metadata.Censored)
				AppendAttribute(sb, "censored", "1");
			AppendAttribute(sb, "tvg-id", metadata.TvgId);
			AppendAttribute(sb, "tvg-name", metadata.TvgName);
			AppendAttribute(sb, "tvg-logo", metadata.TvgLogo);
			AppendAttribute(sb, "group_id", metadata.GroupId);
			AppendAttribute(sb, "group-title", metadata.GroupTitle);
			AppendRaw(sb, metadata.AdditionalAttributes);
			sb.Append(',').Append(entry.Title).Append('\n');

			if (entry.Grouping is not null)
				sb.Append(ExtGrp).Append(entry.Grouping).Append('\n');

			if (!string.IsNullOrWhiteSpace(entry.Directives))
				sb.Append(entry.Directives.Trim()).Append('\n');

			sb.Append(entry.FilePath).Append('\n');
		}

		return sb.ToString();
	}

	private static void ReadPlaylistAttributes(PlaylistModel playlist, string text)
	{
		List<string> additional = [];

		foreach ((string key, string value, string raw) in ParseAttributes(text))
		{
			switch (key.ToUpperInvariant())
			{
				case "URL-TVG":
					playlist.UrlTvg = value;
					break;
				case "CACHE":
					if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cache))
						playlist.Cache = cache;
					break;
				case "REFRESH":
					if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int refresh))
						playlist.Refresh = refresh;
					break;
				case "DEINTERLACE":
					if (Enum.TryParse(value, true, out Deinterlace deinterlace) && Enum.IsDefined(deinterlace))
						playlist.Deinterlace = deinterlace;
					break;
				default:
					additional.Add(raw);
					break;
			}
		}

		playlist.AdditionalAttributes = JoinOrNull(additional);
	}

	private static EntryModel ReadEntry(string text)
	{
		// The title starts after the first comma that is not inside a quoted attribute value.
		int titleIndex = IndexOfUnquoted(text, ',');
		string info = titleIndex >= 0 ? text[..titleIndex] : text;
		string title = titleIndex >= 0 ? text[(titleIndex + 1)..].Trim() : string.Empty;

		int durationEnd = info.IndexOfAny([' ', '\t']);
		string durationText = durationEnd >= 0 ? info[..durationEnd] : info;
		string attributeText = durationEnd >= 0 ? info[durationEnd..] : string.Empty;

		MetadataModel metadata = new();
		List<string> additional = [];

		foreach ((string key, string value, string raw) in ParseAttributes(attributeText))
		{
			switch (key.ToUpperInvariant())
			{
				case "CENSORED":
					metadata.Censored = value == "1";
					break;
				case "TVG-ID":
					metadata.TvgId = value;
					break;
				case "TVG-NAME":
					metadata.TvgName = value;
					break;
				case "TVG-LOGO":
					metadata.TvgLogo = value;
					break;
				case "GROUP_ID":
					metadata.GroupId = value;
					break;
				case "GROUP-TITLE":
					metadata.GroupTitle = value;
					break;
				default:
					additional.Add(raw);
					break;
			}
		}

		metadata.AdditionalAttributes = JoinOrNull(additional);

		return new EntryModel(title, string.Empty, ParseDuration(durationText), null, metadata);
	}

	private static int ParseDuration(string text)
	{
		if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int duration))
			return duration;

		// Some players write fractional seconds, e.g. "#EXTINF:10.5,".
		return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal fractional)
			? (int)fractional
			: -1;
	}

	/// <summary>
	/// Reads <c>key="value"</c> and <c>key=value</c> pairs, matching keys exactly instead of by substring.
	/// </summary>
	private static IEnumerable<(string Key, string Value, string Raw)> ParseAttributes(string text)
	{
		foreach (Match match in AttributeRegex().Matches(text))
		{
			string value = match.Groups["quoted"].Success ? match.Groups["quoted"].Value : match.Groups["plain"].Value;
			yield return (match.Groups["key"].Value, value, match.Value);
		}
	}

	private static int IndexOfUnquoted(string text, char character)
	{
		bool quoted = false;

		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '"')
				quoted = !quoted;
			else if (!quoted && text[i] == character)
				return i;
		}

		return -1;
	}

	private static void AppendAttribute(StringBuilder sb, string key, string? value)
	{
		if (value is not null)
			sb.Append(' ').Append(key).Append("=\"").Append(value).Append('"');
	}

	private static void AppendRaw(StringBuilder sb, string? text)
	{
		if (!string.IsNullOrWhiteSpace(text))
			sb.Append(' ').Append(text.Trim());
	}

	private static string? JoinOrNull(List<string> values)
		=> values.Count > 0 ? string.Join(' ', values) : null;

	[GeneratedRegex("""(?<key>[^\s=",]+)=(?:"(?<quoted>[^"]*)"|(?<plain>[^\s",]*))""")]
	private static partial Regex AttributeRegex();
}