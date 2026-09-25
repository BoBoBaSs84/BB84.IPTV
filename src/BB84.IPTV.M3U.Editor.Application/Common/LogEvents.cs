// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Common;

/// <summary>
/// Holds the event ids of the entries the application writes to the log.
/// </summary>
/// <remarks>
/// One thousand ids per category, so an entry says where it came from: 1000 the application
/// lifecycle, 2000 the database, 3000 the file operations, 4000 the web requests, 5000 the logos,
/// 6000 the settings and paths, 7000 the events and 8000 the presentation. The first id of a range
/// stays free, so an entry without an id is never mistaken for one of them. Every id is used once
/// and stays inside the range of its category, which <c>LogEventsTests</c> asserts over the ids of
/// this class; the source generator does not check either.
/// </remarks>
public static class LogEvents
{
	/// <summary>
	/// Holds the event ids of the application lifecycle, 1000 to 1999.
	/// </summary>
	public static class Application
	{
		/// <summary>
		/// The application is starting.
		/// </summary>
		public const int Starting = 1001;

		/// <summary>
		/// The application is exiting.
		/// </summary>
		public const int Exiting = 1002;

		/// <summary>
		/// An exit of the application was requested.
		/// </summary>
		public const int ExitRequested = 1003;

		/// <summary>
		/// A restart of the application was requested.
		/// </summary>
		public const int RestartRequested = 1004;

		/// <summary>
		/// An exception nobody handled reached the dispatcher.
		/// </summary>
		public const int UnhandledException = 1005;
	}

	/// <summary>
	/// Holds the event ids of the database, 2000 to 2999.
	/// </summary>
	/// <remarks>
	/// The range is reserved, the database services report what happens through the event service.
	/// </remarks>
	public static class Database
	{
	}

	/// <summary>
	/// Holds the event ids of the file operations, 3000 to 3999.
	/// </summary>
	public static class File
	{
		/// <summary>
		/// A playlist is being loaded from a file.
		/// </summary>
		public const int PlaylistLoading = 3001;

		/// <summary>
		/// A playlist was loaded from a file.
		/// </summary>
		public const int PlaylistLoaded = 3002;

		/// <summary>
		/// A playlist is being saved to a file.
		/// </summary>
		public const int PlaylistSaving = 3003;

		/// <summary>
		/// A playlist was saved to a file.
		/// </summary>
		public const int PlaylistSaved = 3004;
	}

	/// <summary>
	/// Holds the event ids of the web requests, 4000 to 4999.
	/// </summary>
	public static class Web
	{
		/// <summary>
		/// A request was sent to the api.
		/// </summary>
		public const int ApiRequestSent = 4001;

		/// <summary>
		/// The api answered with items.
		/// </summary>
		public const int ApiItemsReceived = 4002;

		/// <summary>
		/// The api answered without items.
		/// </summary>
		public const int ApiNoItemsReceived = 4003;
	}

	/// <summary>
	/// Holds the event ids of the logos, 5000 to 5999.
	/// </summary>
	public static class Logo
	{
		/// <summary>
		/// A logo could not be downloaded.
		/// </summary>
		public const int DownloadFailed = 5001;

		/// <summary>
		/// The host refused a logo.
		/// </summary>
		public const int DownloadRefused = 5002;

		/// <summary>
		/// A logo could not be cached.
		/// </summary>
		public const int CacheFailed = 5003;

		/// <summary>
		/// The logo cache could not be read.
		/// </summary>
		public const int CacheRefreshFailed = 5004;
	}

	/// <summary>
	/// Holds the event ids of the settings and the paths, 6000 to 6999.
	/// </summary>
	/// <remarks>
	/// The range is reserved, the settings service reports what happens through the event service.
	/// </remarks>
	public static class Settings
	{
	}

	/// <summary>
	/// Holds the event ids of the events, 7000 to 7999.
	/// </summary>
	public static class Events
	{
		/// <summary>
		/// An event is being published.
		/// </summary>
		public const int Published = 7001;
	}

	/// <summary>
	/// Holds the event ids of the presentation, 8000 to 8999.
	/// </summary>
	public static class Presentation
	{
		/// <summary>
		/// An error was shown to the user.
		/// </summary>
		public const int NotificationError = 8001;

		/// <summary>
		/// A warning was shown to the user.
		/// </summary>
		public const int NotificationWarning = 8002;

		/// <summary>
		/// An information was shown to the user.
		/// </summary>
		public const int NotificationInformation = 8003;
	}
}
