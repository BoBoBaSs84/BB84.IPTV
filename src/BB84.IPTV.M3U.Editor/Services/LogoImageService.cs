// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.Concurrent;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Services;

/// <summary>
/// Turns the <c>tvg-logo</c> of an entry or the logo URL of a catalog channel into an image.
/// </summary>
/// <remarks>
/// Only cached files are read, a URL that is not cached stays without an image; the views never
/// cause a download. The map of cached files is read once and again after a cache run.
/// </remarks>
public sealed class LogoImageService
{
	private readonly ILogoService _logoService;
	private readonly ConcurrentDictionary<string, IImage?> _imagesByPath = new(StringComparer.OrdinalIgnoreCase);
	private IReadOnlyDictionary<string, string> _pathsByUrl = new Dictionary<string, string>();

	/// <summary>
	/// Initializes a new instance of the <see cref="LogoImageService"/> class.
	/// </summary>
	/// <param name="logoService">The service that knows which logos are cached.</param>
	/// <param name="eventService">The service that reports a changed logo cache.</param>
	public LogoImageService(ILogoService logoService, IEventService eventService)
	{
		ArgumentNullException.ThrowIfNull(eventService);

		_logoService = logoService;

		eventService.Subscribe<LogoCacheChangedEvent>(OnLogoCacheChanged);
	}

	/// <summary>
	/// Gets the service the views use, set once the host is built.
	/// </summary>
	/// <remarks>
	/// A value converter cannot be given dependencies, so it reaches the service through here.
	/// </remarks>
	public static LogoImageService? Current { get; internal set; }

	/// <summary>
	/// Reads which logos are cached, and forgets the images that were loaded before.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task RefreshAsync()
	{
		_pathsByUrl = await _logoService
			.GetPathsByUrlAsync()
			.ConfigureAwait(false);

		_imagesByPath.Clear();
	}

	private void OnLogoCacheChanged(LogoCacheChangedEvent @event)
		=> _ = RefreshAsync();

	/// <summary>
	/// Gets the image of a logo value, which is either a cached file or the URL it came from.
	/// </summary>
	/// <param name="logo">The <c>tvg-logo</c> of an entry or the URL of a catalog logo.</param>
	/// <returns>The image, or <see langword="null"/> if the logo is not cached.</returns>
	public IImage? GetImage(string? logo)
	{
		if (string.IsNullOrWhiteSpace(logo))
			return null;

		string? path = _pathsByUrl.TryGetValue(logo, out string? cached) ? cached : logo;

		return !File.Exists(path)
			? null
			: _imagesByPath.GetOrAdd(path, Load);
	}

	/// <summary>
	/// Loads a cached file, an SVG through the Skia based SVG image, everything else as a bitmap.
	/// </summary>
	private static IImage? Load(string path)
	{
		try
		{
			if (Path.GetExtension(path).Equals(".svg", StringComparison.OrdinalIgnoreCase))
				return new SvgImage { Source = SvgSource.Load(path) };

			return new Bitmap(path);
		}
		catch (Exception)
		{
			// A broken or unsupported file is shown as no logo at all, it must not break a list.
			return null;
		}
	}
}