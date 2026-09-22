// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the store that holds the downloaded logo files.
/// </summary>
/// <remarks>
/// The files live in <c>&lt;app data&gt;/logos/&lt;channel&gt;</c>, so a channel keeps its logos
/// together and a cleared cache is one directory to delete.
/// </remarks>
internal sealed class LogoStoreService : ILogoStoreService
{
	public async Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channel);
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		ArgumentNullException.ThrowIfNull(content);

		string directory = Path.Combine(ApplicationPaths.LogoDirectory, MakeSafe(channel));
		_ = Directory.CreateDirectory(directory);

		string filePath = Path.Combine(directory, MakeSafe(fileName));

		await File.WriteAllBytesAsync(filePath, content, cancellationToken)
			.ConfigureAwait(false);

		return filePath;
	}

	public bool Exists(string? path)
		=> !string.IsNullOrWhiteSpace(path) && File.Exists(path);

	public int Clear()
	{
		if (!Directory.Exists(ApplicationPaths.LogoDirectory))
			return 0;

		int deleted = Directory.GetFiles(ApplicationPaths.LogoDirectory, "*", SearchOption.AllDirectories).Length;
		Directory.Delete(ApplicationPaths.LogoDirectory, true);

		return deleted;
	}

	/// <summary>
	/// Replaces the characters a file system does not take, e.g. the <c>@</c> separator is fine but
	/// a slash in a channel identifier is not.
	/// </summary>
	private static string MakeSafe(string name)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		string safe = new([.. name.Select(character => invalid.Contains(character) ? '_' : character)]);

		return safe.Trim().Trim('.') is { Length: > 0 } trimmed ? trimmed : "unnamed";
	}
}