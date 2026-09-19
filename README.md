# BB84.IPTV

A desktop application to view, create, edit and merge IPTV playlists (M3U). Channel data comes from the public [iptv-org API](https://github.com/iptv-org/api), is stored in a local SQLite database and can be combined with your own custom channels. The app can also generate a `channels.xml` for [iptv-org/epg](https://github.com/iptv-org/epg#usage), so that your custom channel list gets matching program guide data.

> **Status:** work in progress. The current version is a WPF app that imports the iptv-org data into SQLite and edits a single M3U file. This document describes where the project is heading.

## Goals

- **View, create, edit and merge playlists.** Playlists are stored in the database. M3U is the import/export format.
- **Choose from the iptv-org catalog or add custom channels.** Custom channels accept any stream URL, for example `rtsp://192.168.12.1:554` for a stream from somewhere else in your network.
- **Local logo cache.** Channel logos are downloaded once and referenced locally, so loading a playlist needs no network round trips for logos.
- **`channels.xml` export.** Create a custom channel list (`site`, `site_id`, `lang`, `xmltv_id`) for the iptv-org EPG grabber.
- **SQLite as the single store** for catalog data, playlists, custom channels and logo cache metadata.
- **AvaloniaUI** as the UI framework, so the app runs on Windows, Linux and macOS.

## Data sources

| Source                                                                     | Used for                                                                                                                         |
| -------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| [`https://iptv-org.github.io/api/*.json`](https://github.com/iptv-org/api) | Channels, feeds, streams, guides, logos, categories, languages, countries. Already imported by `WebService` / `DatabaseService`. |
| [iptv-org/epg](https://github.com/iptv-org/epg#usage)                      | Format of `channels.xml`.                                                                                                        |
| `misc/epg-de.xml(.gz)`                                                     | German EPG snapshot, refreshed weekly by `.github/workflows/epg.yml`. Not part of the app.                                       |

## Architecture

Clean-architecture layering, dependencies point inward:

```
Host (WPF now, Avalonia next)  →  Infrastructure  →  Application  →  Domain
```

- **Domain**: EF entities and playlist models.
- **Application**: interfaces, services, MVVM view models, events, settings, localized resources. View models stay UI-framework-free, so they carry over to Avalonia unchanged.
- **Infrastructure**: EF Core / SQLite, iptv-org HTTP client, file, settings and logging services.
- **Host**: UI and the presentation service implementations (`IFileDialogService`, `INotificationService`, `IUserService`).

The Avalonia host is added as a new project (`BB84.IPTV.M3U.Editor.Avalonia`) beside the WPF host. Views are ported one by one following the [WPF migration guide](https://docs.avaloniaui.net/docs/migration/wpf/), and the WPF host is deleted once feature parity is reached. See [CLAUDE.md](CLAUDE.md) for the current code layout and conventions.

## Roadmap

### Phase 0 – Cross-platform groundwork

- Change the target framework in `Directory.Build.props` from `net10.0-windows` to `net10.0`.
- Replace the Windows EventLog logger (`Microsoft.Extensions.Logging.EventLog`) used in Production with a cross-platform sink.
- Move the SQLite file from the working directory to a per-user application data folder (`RegisterDatabaseContext` in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`).
- Remove the unused `System.Configuration.ConfigurationManager` reference from Application.
- Keep `System.Drawing` / `IconExtensions` in the WPF host only; it disappears together with that host.

**Done when:** Application, Domain, Infrastructure and their tests build and pass on Linux and Windows.

### Phase 1 – Avalonia host

- New project with `App.axaml`, the same host/DI setup (reusing the three `DependencyInjectionInstaller`s) and Avalonia implementations of the presentation services.
- Port `MainWindow`, `AboutControl`, `DatabaseControl`, `SettingsControl`, `PlaylistControl` and `IntegerTextBoxBehavior`.
- Keep the existing localization resources (de, es, fr, it).
- Remove the WPF host and its solution entry at the end.

**Done when:** the Avalonia app offers everything the WPF app does and the WPF project is gone.

### Phase 2 – Playlist persistence

- New entities `PlaylistEntity` and `PlaylistEntryEntity` (position, title, group, `tvg-*` metadata, optional reference to a channel, stream or custom channel), with EF configurations and repositories following the existing `Infrastructure/Persistence` patterns.
- `IPlaylistService` for load/save and for M3U import/export through `SerializerService`.
- Fix known serializer gaps first:
  - `#EXTGRP` is written without a line break.
  - `GetMetadataValue` finds keys by substring, so attribute names that contain other names can be mixed up.
  - `Deserialize(byte[])` only splits on `\r\n` and `\n`.

**Done when:** an M3U file round-trips import → database → export without losing entries or metadata.

### Phase 3 – Playlist editor

- Playlist list: create, rename, delete, import, export.
- Entry grid: add, remove, reorder, edit title, group, `tvg-id`, `tvg-name`, URL.
- Dirty tracking and validation (reuse the logic in `PlaylistViewModel`).

**Done when:** a playlist can be built and edited entirely inside the app.

### Phase 4 – Channel catalog and custom channels

- Searchable, filterable catalog over `ChannelEntity` with its streams and logos (country, language, category, NSFW flag).
- Add catalog channels to a playlist.
- `CustomChannelEntity` (name, URL, group, logo, `tvg-id`) for streams that are not in the catalog. Any URL scheme is allowed (`http`, `https`, `rtsp`, `udp`, ...).

**Done when:** a playlist can mix catalog channels and custom channels such as `rtsp://192.168.12.1:554`.

### Phase 5 – Merge

- Merge two or more playlists.
- Options: append, de-duplicate by `tvg-id` or URL, keep first or last, remap groups.
- Preview the result before committing.

**Done when:** merging produces a new playlist and never modifies the sources.

### Phase 6 – Local logo cache

- `ILogoService` downloads `LogoEntity.Url` to `<app data>/logos/<channel>/<file>` and stores the local path plus a hash/ETag in the database.
- Picks the best logo per channel from tags, size and format.
- Bulk download with progress (`ProgressChangedEvent`), cancellable and resumable.
- M3U export writes `tvg-logo` as a local path (absolute or relative, configurable).
- The UI shows cached logos only; missing logos never trigger network calls at playlist load.

**Done when:** loading and displaying a playlist works offline with all logos shown, including SVG and WebP.

### Phase 7 – `channels.xml` export

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

### Phase 8 – Polish

- Settings for data paths and logo policy.
- Consistent error, warning and progress notifications.
- CI workflow for build and test.
- Update `CLAUDE.md` to the final architecture.

## Data model (planned additions)

| Entity                            | Purpose                                                        | Key relations                                                     |
| --------------------------------- | -------------------------------------------------------------- | ----------------------------------------------------------------- |
| `PlaylistEntity`                  | Playlist header (name, `url-tvg`, cache, refresh, deinterlace) | 1 → n `PlaylistEntryEntity`                                       |
| `PlaylistEntryEntity`             | Ordered entry with title, group, `tvg-*` metadata, URL         | optional → `ChannelEntity`, `StreamEntity`, `CustomChannelEntity` |
| `CustomChannelEntity`             | User-defined channel and stream URL                            | referenced by entries                                             |
| `ChannelGuideMappingEntity`       | `site` / `site_id` / `lang` / `xmltv_id` for `channels.xml`    | per entry, prefilled from `GuideEntity`                           |
| Logo cache fields on `LogoEntity` | Local path, hash/ETag, downloaded-at                           | 1 → 1 with existing logo row                                      |

## Build and run

```bash
dotnet build BB84.IPTV.slnx
dotnet test BB84.IPTV.slnx
dotnet run --project src/BB84.IPTV.M3U.Editor
```

Requires the .NET 10 SDK.

## License

[MIT](LICENSE). See also the [Code of Conduct](CODE_OF_CONDUCT.md).
