# BB84.IPTV

[![Build](https://github.com/BoBoBaSs84/BB84.IPTV/actions/workflows/build.yml/badge.svg)](https://github.com/BoBoBaSs84/BB84.IPTV/actions/workflows/build.yml)

A desktop application to view, create, edit and merge IPTV playlists (M3U). Channel data comes from the public [iptv-org API](https://github.com/iptv-org/api), is stored in a local SQLite database and can be combined with your own custom channels. The app can also generate a `channels.xml` for [iptv-org/epg](https://github.com/iptv-org/epg#usage), so that your custom channel list gets matching program guide data.

> **Status:** work in progress. The current version is a cross-platform AvaloniaUI app that imports the iptv-org data into SQLite, stores playlists in the database, edits them (import/export as M3U) and fills them from the channel catalog or from custom channels. This document describes where the project is heading.

## Goals

- **View, create, edit and merge playlists.** Playlists are stored in the database. M3U is the import/export format.
- **Choose from the iptv-org catalog or add custom channels.** Custom channels accept any stream URL, for example `rtsp://192.168.12.1:554` for a stream from somewhere else in your network.
- **Local logo cache.** Channel logos are downloaded once and referenced locally, so loading a playlist needs no network round trips for logos.
- **`channels.xml` export.** Create a custom channel list (`site`, `site_id`, `lang`, `xmltv_id`) for the iptv-org EPG grabber.
- **SQLite as the single store** for catalog data, playlists, custom channels and logo cache metadata.
- **AvaloniaUI** as the UI framework, so the app runs on Windows, Linux and macOS.

## Data sources

| Source                                                                     | Used for                                                                                                                                   |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| [`https://iptv-org.github.io/api/*.json`](https://github.com/iptv-org/api) | Channels, feeds, streams, guides, logos, categories, languages, countries. Already imported by `WebService` / `DatabaseService`.           |
| [iptv-org/epg](https://github.com/iptv-org/epg#usage)                      | Format of `channels.xml`.                                                                                                                  |
| `misc/epg-de.xml(.gz)`                                                     | German EPG snapshot, refreshed weekly by `.github/workflows/epg.yml` on the `epg` branch, which opens a pull request. Not part of the app. |

## Architecture

Clean-architecture layering, dependencies point inward:

```
Host (Avalonia)  →  Infrastructure  →  Application  →  Domain
```

- **Domain**: EF entities and playlist models.
- **Application**: interfaces, services, MVVM view models, events, settings, localized resources. View models stay UI-framework-free.
- **Infrastructure**: EF Core / SQLite, iptv-org HTTP client, file, settings and logging services.
- **Host**: UI and the presentation service implementations (`IFileDialogService`, `INotificationService`, `IUserService`).

The Avalonia host replaced the former WPF host in `src/BB84.IPTV.M3U.Editor` (Phase 1). See [CLAUDE.md](CLAUDE.md) for the current code layout and conventions.

## Roadmap

### Phase 0 – Cross-platform groundwork (done)

- Change the target framework in `Directory.Build.props` from `net10.0-windows` to `net10.0`.
- Replace the Windows EventLog logger (`Microsoft.Extensions.Logging.EventLog`) used in Production with a cross-platform sink.
- Move the SQLite file from the working directory to a per-user application data folder (`RegisterDatabaseContext` in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`).
- Remove the unused `System.Configuration.ConfigurationManager` reference from Application.
- Keep `System.Drawing` / `IconExtensions` in the WPF host only; it disappears together with that host.

**Done when:** Application, Domain, Infrastructure and their tests build and pass on Linux and Windows.

### Phase 1 – Avalonia host (done)

- New project with `App.axaml`, the same host/DI setup (reusing the three `DependencyInjectionInstaller`s) and Avalonia implementations of the presentation services.
- Port `MainWindow`, `AboutControl`, `DatabaseControl`, `SettingsControl`, `PlaylistControl` and `IntegerTextBoxBehavior`.
- Keep the existing localization resources (de, es, fr, it).
- Remove the WPF host and its solution entry at the end.

**Done when:** the Avalonia app offers everything the WPF app does and the WPF project is gone.

### Phase 2 – Playlist persistence (done)

- New entities `PlaylistEntity` and `PlaylistEntryEntity` (position, title, group, `tvg-*` metadata, optional reference to a channel, stream or custom channel), with EF configurations and repositories following the existing `Infrastructure/Persistence` patterns.
- `IPlaylistService` for load/save and for M3U import/export through `SerializerService`.
- Fix known serializer gaps first:
  - `#EXTGRP` is written without a line break.
  - `GetMetadataValue` finds keys by substring, so attribute names that contain other names can be mixed up.
  - `Deserialize(byte[])` only splits on `\r\n` and `\n`.
- Also done: the schema is managed by EF Core migrations (applied at startup), and "Create Database" only resets the iptv-org catalog, so stored playlists survive a catalog re-import. Entries reference catalog channels by their iptv-org id instead of a foreign key. Unknown `#EXTINF` attributes and directives such as `#EXTVLCOPT` are kept, so nothing is lost on export.

**Done when:** an M3U file round-trips import → database → export without losing entries or metadata.

### Phase 3 – Playlist editor (done)

- Playlist list: create, rename, delete, import, export.
- Entry grid: add, remove, reorder, edit title, group, `tvg-id`, `tvg-name`, URL.
- Dirty tracking and validation (reuse the logic in `PlaylistViewModel`).

**Done when:** a playlist can be built and edited entirely inside the app.

### Phase 4 – Channel catalog and custom channels (done)

- Searchable, filterable catalog over `ChannelEntity` with its streams and logos (country, language, category, NSFW flag).
- Add catalog channels to a playlist, one by one or all results at once. The entry keeps the iptv-org `Channel` and `Feed` id, so it stays linked to the catalog.
- `CustomChannelEntity` (name, URL, group, logo, `tvg-id`) for streams that are not in the catalog. Any URL scheme is allowed (`http`, `https`, `rtsp`, `udp`, ...). Custom channels survive a catalog reset.
- New screen "Channels" (menu `Tools`), with the catalog and the custom channels on two tabs.
- The catalog result is paged (`Features/PagedList`, 500 channels per page) and reports the total, so an unfiltered search is navigable instead of silently truncated. Playlists and custom channels are paged as well, in SQL; their paging row only appears when there is more than one page.

**Done when:** a playlist can mix catalog channels and custom channels such as `rtsp://192.168.12.1:554`.

### Phase 5 – Merge (done)

- Merge two or more playlists, in a chosen order (`IMergeService`, screen "Merge Playlists" in the `Tools` menu).
- Options: append, de-duplicate by `tvg-id`, by URL or by either, keep the first or the last of two duplicates, rename or clear groups.
- Preview the result before committing: the merged entries, the number of dropped duplicates and the groups of the result, which is where the renaming is set up.
- The header (`url-tvg`, cache, deinterlace, refresh) of the first source is kept, an entry without a `tvg-id` or URL is never a duplicate.

**Done when:** merging produces a new playlist and never modifies the sources.

### Phase 6 – Local logo cache (done)

- `ILogoService` downloads `LogoEntity.Url` to `<app data>/logos/<channel>/<file>` and stores the local path, the ETag, a content hash, the size and the download time in the database.
- `LogoSelector` picks one logo per channel: the one of the feed, then the one without special tags, then the format that displays best, then the larger image.
- Bulk download from the `Logo Cache` section of the Database screen, with progress, Cancel and Clear. Logos are downloaded in batches (`Logo.MaxParallelDownloads`, four by default, at most 16), a run skips what is already on disk, so it is resumable, and asks with the ETag when refreshing.
- M3U export writes `tvg-logo` as a local path, absolute or relative (`Logo` settings section); the stored playlist keeps its URLs.
- Catalog list and playlist editor show a logo column that reads the cache only, never the network. SVG renders through `Svg.Controls.Skia.Avalonia`, WebP through Skia.

**Done when:** loading and displaying a playlist works offline with all logos shown, including SVG and WebP.

### Phase 7 – `channels.xml` export (done)

- Output format as used by iptv-org/epg:
  ```xml
  <?xml version="1.0" encoding="UTF-8"?>
  <channels>
    <channel site="example.com" site_id="123" lang="de" xmltv_id="Das Erste.de">Das Erste</channel>
  </channels>
  ```
- Per playlist entry a mapping of `site`, `site_id`, `lang` and `xmltv_id` (the entry's `tvg-id`).
- Prefill from imported `GuideEntity` rows (`Site`, `SiteId`, `Lang`, `Channel`); the user can override every value, and custom channels are mapped by hand.
- XML writer with proper escaping, export dialog, tests against the format in the iptv-org/epg documentation.

**Done when:** an exported `channels.xml` is accepted by the iptv-org/epg grabber for a sample playlist.

### Phase 8 – Polish (done)

- Settings for data paths: the `Paths` section of the settings file keeps the directory of the database, of the cached logos and of the log files. An empty value means the default below the per-user data directory, a path that is not fully qualified falls back to it as well, and the settings file itself always stays in the default directory, because it is what tells the application where everything else lives. `IPathService` resolves the paths once while the application starts, the settings screen shows the ones in use and offers a folder dialog per path, and a changed path asks for a restart.
- Settings for the logo policy: the settings screen holds the `Logo` section as well, so whether an export writes the cached file instead of the URL, whether that path is absolute or relative, and how many logos are downloaded at once are set in the UI, the last one capped at `LogoCacheRequest.MaxParallelLimit`.
- Consistent error, warning and progress notifications: a failure is reported by publishing `ErrorOccuredEvent`/`WarningOccuredEvent`/`InformationOccuredEvent` with a localized message and, for an error, the exception; `NotificationService` shows it and writes it to the log, so a publisher never logs what it reports. Every long running operation reports `ProgressChangedEvent` as well, so the status bar of the main window follows the database import like it follows the logo cache.
- CI workflow for build and test: `.github/workflows/build.yml` builds the solution and runs the tests on Linux and Windows for every push to `main` and every pull request.

## Data model (planned additions)

| Entity                            | Purpose                                                        | Key relations                                       |
| --------------------------------- | -------------------------------------------------------------- | --------------------------------------------------- |
| `PlaylistEntity`                  | Playlist header (name, `url-tvg`, cache, refresh, deinterlace) | 1 → n `PlaylistEntryEntity`                         |
| `PlaylistEntryEntity`             | Ordered entry with title, group, `tvg-*` metadata, URL         | iptv-org `Channel`/`Feed` id                        |
| `CustomChannelEntity`             | User-defined channel and stream URL                            | copied into an entry when it is added to a playlist |
| `ChannelGuideMappingEntity`       | `site` / `site_id` / `lang` / `xmltv_id` for `channels.xml`    | per entry, prefilled from `GuideEntity`             |
| Logo cache fields on `LogoEntity` | Local path, hash/ETag, downloaded-at                           | 1 → 1 with existing logo row                        |

## Build and run

```bash
dotnet build BB84.IPTV.slnx
dotnet test BB84.IPTV.slnx
dotnet run --project src/BB84.IPTV.M3U.Editor
```

Requires the .NET 10 SDK.

## License

[MIT](LICENSE). See also the [Code of Conduct](CODE_OF_CONDUCT.md).
