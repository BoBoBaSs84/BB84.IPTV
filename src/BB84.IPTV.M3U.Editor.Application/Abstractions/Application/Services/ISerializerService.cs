// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

namespace BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;

/// <summary>
/// Represents a serializer contract for M3U playlists, providing methods to serialize and deserialize playlist data.
/// </summary>
public interface ISerializerService
{
	/// <summary>
	/// Deserializes an M3U playlist file from the specified file path and returns a <see cref="IPlaylist"/> object.
	/// </summary>
	/// <remarks>
	/// The method reads the M3U file line by line, extracting playlist-level attributes, entry metadata and file paths.
	/// It supports extended M3U attributes such as <c>url-tvg</c>, <c>refresh</c>, and <c>cache</c>, as well as
	/// entry-level metadata like <c>tvg-id</c>, <c>tvg-name</c>, <c>tvg-logo</c>, <c>group-title</c>, and <c>group_id</c>.
	/// Lines starting with <c>#EXTINF:</c> are treated as metadata for the next entry, while lines starting with
	/// <c>#EXTGRP:</c> specify grouping information. Non-comment lines are interpreted as file paths for playlist entries.
	/// </remarks>
	/// <param name="filePath">The path to the M3U file to deserialize. The file must be a valid M3U playlist.</param>
	/// <returns>
	/// A <see cref="IPlaylist"/> object representing the deserialized M3U file, including its entries and metadata.
	/// </returns>
	/// <exception cref="InvalidDataException">Thrown if the file is not a valid M3U playlist.</exception>
	IPlaylist Deserialize(string filePath);

	/// <summary>
	/// Deserializes an M3U playlist from the provided array of file lines and returns a <see cref="IPlaylist"/> object.
	/// </summary>
	/// <remarks>
	/// The method reads line by line, extracting playlist-level attributes, entry metadata and file paths.
	/// It supports extended M3U attributes such as <c>url-tvg</c>, <c>refresh</c>, and <c>cache</c>, as well as
	/// entry-level metadata like <c>tvg-id</c>, <c>tvg-name</c>, <c>tvg-logo</c>, <c>group-title</c>, and <c>group_id</c>.
	/// Lines starting with <c>#EXTINF:</c> are treated as metadata for the next entry, while lines starting with
	/// <c>#EXTGRP:</c> specify grouping information. Non-comment lines are interpreted as file paths for playlist entries.
	/// </remarks>
	/// <param name="fileLines">The lines of the M3U file to deserialize. The lines must represent a valid M3U playlist.</param>
	/// <returns>
	/// A <see cref="IPlaylist"/> object representing the deserialized M3U file, including its entries and metadata.
	/// </returns>
	/// <exception cref="InvalidDataException">Thrown if the file is not a valid M3U playlist.</exception>
	IPlaylist Deserialize(string[] fileLines);

	/// <summary>
	/// Deserializes an M3U playlist from the provided byte array content and returns a <see cref="IPlaylist"/> object.
	/// </summary>
	/// <remarks>
	/// The method reads the M3U file content line by line, extracting playlist-level attributes, entry metadata and file
	/// paths. It supports extended M3U attributes such as <c>url-tvg</c>, <c>refresh</c>, and <c>cache</c>, as well as
	/// entry-level metadata like <c>tvg-id</c>, <c>tvg-name</c>, <c>tvg-logo</c>, <c>group-title</c>, and <c>group_id</c>.
	/// Lines starting with <c>#EXTINF:</c> are treated as metadata for the next entry, while lines starting with
	/// <c>#EXTGRP:</c> specify grouping information. Non-comment lines are interpreted as file paths for playlist entries.
	/// </remarks>
	/// <param name="fileContent">The content of the M3U file as a byte array to deserialize.
	/// The content must represent a valid M3U playlist.</param>
	/// <returns>
	/// A <see cref="IPlaylist"/> object representing the deserialized M3U file, including its entries and metadata.
	/// </returns>
	/// <exception cref="InvalidDataException">Thrown if the file is not a valid M3U playlist.</exception>
	IPlaylist Deserialize(byte[] fileContent);

	/// <summary>
	/// Serializes the provided <paramref name="playlist"/> into an M3U playlist format string.
	/// </summary>
	/// <remarks>
	/// The serialized string includes the M3U header, global attributes such as `url-tvg`, `cache`,
	/// `deinterlace`, and `refresh` (if set), and a list of entries. Each entry includes metadata
	/// (e.g., duration, title, group information) and the file path. The output adheres to the extended
	/// M3U format, including <c>#EXTM3U</c>, <c>#EXTINF</c>, and <c>#EXTGRP</c> directives where applicable.
	/// </remarks>
	/// <param name="playlist">The <see cref="IPlaylist"/> object to serialize into M3U format.</param>
	/// <returns>
	/// A string representing the serialized M3U playlist. The string includes all entries and their
	/// associated metadata, formatted according to the extended M3U specification.
	/// </returns>
	string Serialize(IPlaylist playlist);
}
