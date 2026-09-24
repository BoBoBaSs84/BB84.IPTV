// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the store that holds the downloaded logo files.
/// </summary>
/// <remarks>
/// The files live in the logo directory the <see cref="IPathService"/> provides, one folder per
/// channel, so a channel keeps its logos together and a cleared cache is one directory to delete.
/// </remarks>
/// <param name="pathService">The service that provides the file system locations of the application.</param>
internal sealed class LogoStoreService(IPathService pathService) : ILogoStoreService
{
	public async Task<string> SaveAsync(string channel, string fileName, byte[] content, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(channel);
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		ArgumentNullException.ThrowIfNull(content);

		string directory = Path.Combine(pathService.LogoDirectory, MakeSafe(channel));
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
		if (!Directory.Exists(pathService.LogoDirectory))
			return 0;

		int deleted = Directory.GetFiles(pathService.LogoDirectory, "*", SearchOption.AllDirectories).Length;
		Directory.Delete(pathService.LogoDirectory, true);

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