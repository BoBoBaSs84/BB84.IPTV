// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
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
/// <para>
/// A loaded image holds unmanaged memory and is not freed by the garbage collector, so it has to
/// be released by hand: the cache holds <see cref="CacheCapacity"/> images at most and releases
/// the one that was used longest ago. An image a view still shows is never released, because the
/// rows that are out of view are the ones that fall out of the cache.
/// </para>
/// </remarks>
public sealed class LogoImageService : IDisposable
{
	/// <summary>
	/// The number of loaded images that are kept.
	/// </summary>
	/// <remarks>
	/// The catalog holds far more channels than a screen shows, so the images of the rows that are
	/// no longer looked at are released instead of being kept for the rest of the session.
	/// </remarks>
	internal const int CacheCapacity = 512;

	private readonly ILogoService _logoService;
	private readonly IEventService _eventService;
	private readonly IProviderService _providerService;
	private readonly Lock _lock = new();

	// The order the paths were used in, the one used longest ago comes first.
	private readonly LinkedList<string> _usage = new();
	private readonly Dictionary<string, CacheEntry> _imagesByPath = new(StringComparer.OrdinalIgnoreCase);

	// The images of the run before the last refresh. A view that was not redrawn since still shows
	// them, so they are released one refresh later instead of while they are on the screen.
	private List<IImage?> _retiredImages = [];

	private IReadOnlyDictionary<string, string> _pathsByUrl = new Dictionary<string, string>();

	/// <summary>
	/// Initializes a new instance of the <see cref="LogoImageService"/> class.
	/// </summary>
	/// <param name="logoService">The service that knows which logos are cached.</param>
	/// <param name="eventService">The service that reports a changed logo cache.</param>
	/// <param name="providerService">The provider service used for file access.</param>
	public LogoImageService(ILogoService logoService, IEventService eventService, IProviderService providerService)
	{
		ArgumentNullException.ThrowIfNull(eventService);

		_logoService = logoService;
		_eventService = eventService;
		_providerService = providerService;

		_eventService.Subscribe<LogoCacheChangedEvent>(OnLogoCacheChanged);
	}

	/// <summary>
	/// Gets the service the views use, set once the host is built.
	/// </summary>
	/// <remarks>
	/// A value converter cannot be given dependencies, so it reaches the service through here.
	/// </remarks>
	public static LogoImageService? Current { get; internal set; }

	/// <summary>
	/// Gets the number of images the cache holds.
	/// </summary>
	internal int CachedImageCount
	{
		get
		{
			lock (_lock)
				return _imagesByPath.Count;
		}
	}

	/// <summary>
	/// Reads which logos are cached, and forgets the images that were loaded before.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task RefreshAsync()
	{
		_pathsByUrl = await _logoService
			.GetPathsByUrlAsync()
			.ConfigureAwait(false);

		RetireCache();
	}

	/// <summary>
	/// Releases every image the service holds.
	/// </summary>
	public void Dispose()
	{
		_eventService.Unsubscribe<LogoCacheChangedEvent>(OnLogoCacheChanged);

		lock (_lock)
		{
			foreach (IImage? image in _retiredImages)
				Release(image);

			foreach (CacheEntry entry in _imagesByPath.Values)
				Release(entry.Image);

			_retiredImages = [];
			_imagesByPath.Clear();
			_usage.Clear();
		}
	}

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

		if (!_providerService.File.Exists(path))
			return null;

		lock (_lock)
		{
			if (_imagesByPath.TryGetValue(path, out CacheEntry? entry))
			{
				// Used again, so it is the last one to be released.
				_usage.Remove(entry.Node);
				_usage.AddLast(entry.Node);

				return entry.Image;
			}
		}

		IImage? image = Load(path);

		lock (_lock)
		{
			// Another caller may have loaded the same path in the meantime. Its image is the one
			// the cache keeps, and the one that was loaded twice is released right away.
			if (_imagesByPath.TryGetValue(path, out CacheEntry? existing))
			{
				Release(image);
				return existing.Image;
			}

			_imagesByPath[path] = new CacheEntry(image, _usage.AddLast(path));

			ReleaseLeastRecentlyUsed();

			return image;
		}
	}

	/// <summary>
	/// Releases the images beyond <see cref="CacheCapacity"/>, the one used longest ago first.
	/// </summary>
	/// <remarks>
	/// The caller holds the lock.
	/// </remarks>
	private void ReleaseLeastRecentlyUsed()
	{
		while (_imagesByPath.Count > CacheCapacity && _usage.First is { } oldest)
		{
			_usage.Remove(oldest);

			if (_imagesByPath.Remove(oldest.Value, out CacheEntry? entry))
				Release(entry.Image);
		}
	}

	/// <summary>
	/// Drops the loaded images, so a changed cache is read again.
	/// </summary>
	/// <remarks>
	/// The images of the run before are released now: a view holds the image it was given until it
	/// is redrawn, and by the time a second cache run is through, it has been.
	/// </remarks>
	private void RetireCache()
	{
		lock (_lock)
		{
			foreach (IImage? image in _retiredImages)
				Release(image);

			_retiredImages = [.. _imagesByPath.Values.Select(entry => entry.Image)];
			_imagesByPath.Clear();
			_usage.Clear();
		}
	}

	/// <summary>
	/// Releases what an image holds, which is unmanaged memory for a bitmap as well as for an SVG.
	/// </summary>
	/// <param name="image">The image that is no longer shown.</param>
	private static void Release(IImage? image)
	{
		try
		{
			switch (image)
			{
				case SvgImage svgImage:
					svgImage.Source?.Dispose();
					break;
				case IDisposable disposable:
					disposable.Dispose();
					break;
				default:
					break;
			}
		}
		catch (Exception)
		{
			// An image that is already released is nothing to report, and a logo is never worth
			// ending the application for.
		}
	}

	private void OnLogoCacheChanged(LogoCacheChangedEvent @event)
		=> _ = RefreshAsync();

	/// <summary>
	/// Loads a cached file, an SVG through the Skia based SVG image, everything else as a bitmap.
	/// </summary>
	private IImage? Load(string path)
	{
		try
		{
			if (string.Equals(_providerService.Path.GetExtension(path), ".svg", StringComparison.OrdinalIgnoreCase))
				return new SvgImage { Source = SvgSource.Load(path) };

			return new Bitmap(path);
		}
		catch (Exception)
		{
			// A broken or unsupported file is shown as no logo at all, it must not break a list.
			return null;
		}
	}

	/// <summary>
	/// One loaded image and where it stands in the order the paths were used in.
	/// </summary>
	/// <param name="Image">The loaded image, <see langword="null"/> if the file could not be read.</param>
	/// <param name="Node">The node of the path in the usage order.</param>
	private sealed record CacheEntry(IImage? Image, LinkedListNode<string> Node);
}
