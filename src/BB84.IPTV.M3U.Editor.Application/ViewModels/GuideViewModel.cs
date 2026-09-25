// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Presentation.Services;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the guide screen: which playlist is mapped to which guide sites, and the export of
/// the <c>channels.xml</c> the iptv-org EPG grabber reads.
/// </summary>
public sealed class GuideViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// The file type filter of the <c>channels.xml</c>.
	/// </summary>
	public const string ChannelsFilter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

	/// <summary>
	/// The name the export is suggested with, as the EPG grabber expects it.
	/// </summary>
	public const string ChannelsFileName = "channels.xml";

	/// <summary>
	/// The number of guides of a site that are shown at once.
	/// </summary>
	private const int SiteChannelPageSize = Parameters.MinPageSize;

	private readonly IGuideService _guideService;
	private readonly IPlaylistService _playlistService;
	private readonly IFileDialogService _fileDialogService;
	private readonly IEventService _eventService;
	private PlaylistItemViewModel? _selectedPlaylist;
	private bool _isBusy;
	private string _statusMessage = string.Empty;
	private AsyncActionCommand? _saveCommand;
	private AsyncActionCommand? _exportCommand;
	private GuideMappingViewModel? _selectedMapping;
	private GuideOptionResponse? _selectedOption;
	private GuideSiteResponse? _selectedSite;
	private GuideOptionResponse? _selectedSiteChannel;
	private string _siteSearchText = string.Empty;
	private string _optionsStatusMessage = string.Empty;
	private string _siteStatusMessage = string.Empty;
	private int _siteChannelPageNumber = 1;
	private int _siteChannelTotalPages;
	private bool _sitesLoaded;
	private ActionCommand? _applyOptionCommand;
	private ActionCommand? _applySiteChannelCommand;
	private AsyncActionCommand? _refreshSitesCommand;
	private AsyncActionCommand? _searchSiteChannelsCommand;
	private AsyncActionCommand? _previousSiteChannelPageCommand;
	private AsyncActionCommand? _nextSiteChannelPageCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuideViewModel"/> class.
	/// </summary>
	/// <param name="guideService">The service that maps the entries and writes the file.</param>
	/// <param name="playlistService">The service that stores the playlists.</param>
	/// <param name="fileDialogService">The service that shows file dialogs.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	public GuideViewModel(IGuideService guideService, IPlaylistService playlistService, IFileDialogService fileDialogService, IEventService eventService)
	{
		_guideService = guideService;
		_playlistService = playlistService;
		_fileDialogService = fileDialogService;
		_eventService = eventService;

		// The commands depend on the selection and the rows, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Mappings.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the stored playlists to pick from.
	/// </summary>
	public ObservableCollection<PlaylistItemViewModel> Playlists { get; } = [];

	/// <summary>
	/// Gets the entries of the selected playlist and what they become in the <c>channels.xml</c>.
	/// </summary>
	public ObservableCollection<GuideMappingViewModel> Mappings { get; } = [];

	/// <summary>
	/// Gets the guides the catalog knows for the selected entry, the one of its feed first.
	/// </summary>
	public ObservableCollection<GuideOptionResponse> Options { get; } = [];

	/// <summary>
	/// Gets the sites the program guides are grabbed from.
	/// </summary>
	public ObservableCollection<GuideSiteResponse> Sites { get; } = [];

	/// <summary>
	/// Gets the guides of the selected site, one page of them.
	/// </summary>
	public ObservableCollection<GuideOptionResponse> SiteChannels { get; } = [];

	/// <summary>
	/// Gets or sets the row the guides are shown and applied for.
	/// </summary>
	public GuideMappingViewModel? SelectedMapping
	{
		get => _selectedMapping;
		set
		{
			if (SetProperty(ref _selectedMapping, value))
				_ = LoadOptionsAndReportAsync();
		}
	}

	/// <summary>
	/// Gets or sets the guide that is applied to the selected row.
	/// </summary>
	public GuideOptionResponse? SelectedOption
	{
		get => _selectedOption;
		set => SetProperty(ref _selectedOption, value);
	}

	/// <summary>
	/// Gets or sets the site whose guides are looked through.
	/// </summary>
	public GuideSiteResponse? SelectedSite
	{
		get => _selectedSite;
		set
		{
			if (SetProperty(ref _selectedSite, value))
				_ = LoadSiteChannelsAndReportAsync(1);
		}
	}

	/// <summary>
	/// Gets or sets the guide of the site that is applied to the selected row.
	/// </summary>
	public GuideOptionResponse? SelectedSiteChannel
	{
		get => _selectedSiteChannel;
		set => SetProperty(ref _selectedSiteChannel, value);
	}

	/// <summary>
	/// Gets or sets the text the guides of the selected site are searched with.
	/// </summary>
	public string SiteSearchText
	{
		get => _siteSearchText;
		set => SetProperty(ref _siteSearchText, value);
	}

	/// <summary>
	/// Gets what is known about the guides of the selected row.
	/// </summary>
	public string OptionsStatusMessage
	{
		get => _optionsStatusMessage;
		private set => SetProperty(ref _optionsStatusMessage, value);
	}

	/// <summary>
	/// Gets what the site browser shows, e.g. how many sites are known.
	/// </summary>
	public string SiteStatusMessage
	{
		get => _siteStatusMessage;
		private set => SetProperty(ref _siteStatusMessage, value);
	}

	/// <summary>
	/// Gets the number of the page of guides that is shown, the first page is page one.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousSiteChannelPage), nameof(HasNextSiteChannelPage))]
	public int SiteChannelPageNumber
	{
		get => _siteChannelPageNumber;
		private set => SetProperty(ref _siteChannelPageNumber, value);
	}

	/// <summary>
	/// Gets the number of pages the guides of the selected site have.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousSiteChannelPage), nameof(HasNextSiteChannelPage))]
	public int SiteChannelTotalPages
	{
		get => _siteChannelTotalPages;
		private set => SetProperty(ref _siteChannelTotalPages, value);
	}

	/// <summary>
	/// Indicates whether a page of guides comes before the one that is shown.
	/// </summary>
	public bool HasPreviousSiteChannelPage
		=> SiteChannelPageNumber > 1;

	/// <summary>
	/// Indicates whether a page of guides comes after the one that is shown.
	/// </summary>
	public bool HasNextSiteChannelPage
		=> SiteChannelPageNumber < SiteChannelTotalPages;

	/// <summary>
	/// Gets or sets the playlist whose entries are mapped.
	/// </summary>
	public PlaylistItemViewModel? SelectedPlaylist
	{
		get => _selectedPlaylist;
		set
		{
			if (SetProperty(ref _selectedPlaylist, value))
				_ = LoadMappingsAndReportAsync();
		}
	}

	/// <summary>
	/// Indicates whether the view model is busy performing a long-running operation.
	/// </summary>
	public bool IsBusy
	{
		get => _isBusy;
		private set => SetProperty(ref _isBusy, value);
	}

	/// <summary>
	/// Gets what the screen is doing, e.g. how many entries can be grabbed.
	/// </summary>
	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	/// <summary>
	/// Gets the number of entries that hold everything a <c>channels.xml</c> entry needs.
	/// </summary>
	public int CompleteCount
		=> Mappings.Count(mapping => mapping.IsComplete);

	/// <summary>
	/// Indicates whether a playlist with mapped entries is selected.
	/// </summary>
	public bool HasMappings
		=> Mappings.Count > 0;

	/// <summary>
	/// Gets the command that stores the mappings of the selected playlist.
	/// </summary>
	public IAsyncActionCommand SaveCommand
		=> _saveCommand ??= new AsyncActionCommand(SaveAsync, () => !IsBusy && HasMappings, OnError);

	/// <summary>
	/// Gets the command that writes the <c>channels.xml</c>.
	/// </summary>
	public IAsyncActionCommand ExportCommand
		=> _exportCommand ??= new AsyncActionCommand(ExportAsync, () => !IsBusy && HasMappings, OnError);

	/// <summary>
	/// Gets the command that takes the selected guide into the selected row.
	/// </summary>
	public IActionCommand ApplyOptionCommand
		=> _applyOptionCommand ??= new ActionCommand(ApplyOption, () => !IsBusy && SelectedMapping is not null && SelectedOption is not null);

	/// <summary>
	/// Gets the command that takes the selected guide of the site browser into the selected row.
	/// </summary>
	public IActionCommand ApplySiteChannelCommand
		=> _applySiteChannelCommand ??= new ActionCommand(ApplySiteChannel, () => !IsBusy && SelectedMapping is not null && SelectedSiteChannel is not null);

	/// <summary>
	/// Gets the command that reads the sites again, e.g. after the catalog was imported anew.
	/// </summary>
	public IAsyncActionCommand RefreshSitesCommand
		=> _refreshSitesCommand ??= new AsyncActionCommand(() => LoadSitesAsync(true), () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that searches the guides of the selected site.
	/// </summary>
	public IAsyncActionCommand SearchSiteChannelsCommand
		=> _searchSiteChannelsCommand ??= new AsyncActionCommand(() => LoadSiteChannelsAsync(1), () => !IsBusy && SelectedSite is not null, OnError);

	/// <summary>
	/// Gets the command that shows the page of guides before the one that is shown.
	/// </summary>
	public IAsyncActionCommand PreviousSiteChannelPageCommand
		=> _previousSiteChannelPageCommand ??= new AsyncActionCommand(() => LoadSiteChannelsAsync(SiteChannelPageNumber - 1), () => !IsBusy && HasPreviousSiteChannelPage, OnError);

	/// <summary>
	/// Gets the command that shows the page of guides after the one that is shown.
	/// </summary>
	public IAsyncActionCommand NextSiteChannelPageCommand
		=> _nextSiteChannelPageCommand ??= new AsyncActionCommand(() => LoadSiteChannelsAsync(SiteChannelPageNumber + 1), () => !IsBusy && HasNextSiteChannelPage, OnError);

	/// <summary>
	/// Loads the stored playlists, the selected one stays selected.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		IsBusy = true;

		try
		{
			IPagedList<PlaylistSummaryResponse> playlists = await _playlistService
				.GetPlaylistsAsync(new PlaylistSearchRequest(), cancellationToken)
				.ConfigureAwait(true);

			int? selectedId = SelectedPlaylist?.Id;
			Playlists.Clear();
			foreach (PlaylistSummaryResponse playlist in playlists)
				Playlists.Add(new PlaylistItemViewModel(playlist));

			SelectedPlaylist = Playlists.FirstOrDefault(playlist => playlist.Id == selectedId)
				?? Playlists.FirstOrDefault();
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads like <see cref="LoadAsync"/> and reports a failure instead of throwing.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAndReportAsync()
	{
		try
		{
			await LoadAsync().ConfigureAwait(true);
			await LoadSitesAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Loads the mappings of the selected playlist, prefilled from the imported guides.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadMappingsAsync(CancellationToken cancellationToken = default)
	{
		Mappings.Clear();

		if (SelectedPlaylist is not { } playlist)
		{
			StatusMessage = string.Empty;
			return;
		}

		IsBusy = true;

		try
		{
			IReadOnlyList<GuideMappingResponse> mappings = await _guideService
				.GetMappingsAsync(playlist.Id, cancellationToken)
				.ConfigureAwait(true);

			foreach (GuideMappingResponse mapping in mappings)
			{
				GuideMappingViewModel row = new(mapping);
				row.PropertyChanged += OnMappingPropertyChanged;
				Mappings.Add(row);
			}

			RaiseCountsChanged();
			StatusMessage = Resources.GuideMappingStatus.FormatMessage(CompleteCount, Mappings.Count);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task LoadMappingsAndReportAsync()
	{
		try
		{
			await LoadMappingsAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	private async Task SaveAsync()
	{
		if (SelectedPlaylist is not { } playlist)
			return;

		IsBusy = true;

		try
		{
			int saved = await _guideService
				.SaveMappingsAsync(playlist.Id, Mappings.Select(mapping => mapping.ToResponse()))
				.ConfigureAwait(true);

			StatusMessage = Resources.GuideMappingsSaved.FormatMessage(saved);
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task ExportAsync()
	{
		if (SelectedPlaylist is not { } playlist)
			return;

		string? filePath = await _fileDialogService
			.ShowSaveFileDialogAsync(ChannelsFilter, Resources.GuideExportTitle, ChannelsFileName)
			.ConfigureAwait(true);

		if (filePath is null)
			return;

		IsBusy = true;

		try
		{
			int written = await _guideService
				.ExportAsync(playlist.Id, filePath, Mappings.Select(mapping => mapping.ToResponse()))
				.ConfigureAwait(true);

			StatusMessage = Resources.GuideExported.FormatMessage(written, filePath);
			PublishStatus(StatusMessage);
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads the sites the program guides are grabbed from, once per screen unless a fresh read is
	/// asked for.
	/// </summary>
	/// <param name="force">Reads the sites again although they were read before.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadSitesAsync(bool force = false, CancellationToken cancellationToken = default)
	{
		if (_sitesLoaded && !force)
			return;

		IsBusy = true;

		try
		{
			IReadOnlyList<GuideSiteResponse> sites = await _guideService
				.GetSitesAsync(cancellationToken)
				.ConfigureAwait(true);

			SelectedSite = null;
			Sites.Clear();

			foreach (GuideSiteResponse site in sites)
				Sites.Add(site);

			_sitesLoaded = true;
			SiteStatusMessage = Resources.GuideSitesStatus.FormatMessage(Sites.Count);
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads the guides the catalog knows for the selected row.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadOptionsAsync(CancellationToken cancellationToken = default)
	{
		SelectedOption = null;
		Options.Clear();

		if (SelectedMapping?.Channel is not { Length: > 0 } channel)
		{
			OptionsStatusMessage = Resources.GuideNoOptions;
			return;
		}

		IsBusy = true;

		try
		{
			IReadOnlyList<GuideOptionResponse> options = await _guideService
				.GetOptionsAsync(channel, SelectedMapping.Feed, cancellationToken)
				.ConfigureAwait(true);

			foreach (GuideOptionResponse option in options)
				Options.Add(option);

			// The guide the row holds is picked, so the list shows where the row stands.
			SelectedOption = Options.FirstOrDefault(IsApplied) ?? Options.FirstOrDefault();
			OptionsStatusMessage = Options.Count is 0
				? Resources.GuideNoOptions
				: Resources.GuideOptionsStatus.FormatMessage(Options.Count);
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads a page of the guides the selected site holds.
	/// </summary>
	/// <param name="pageNumber">The page to show, the first page is page one.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadSiteChannelsAsync(int pageNumber, CancellationToken cancellationToken = default)
	{
		SelectedSiteChannel = null;
		SiteChannels.Clear();

		if (SelectedSite is not { } site)
		{
			SiteChannelPageNumber = 1;
			SiteChannelTotalPages = 0;
			return;
		}

		IsBusy = true;

		try
		{
			IPagedList<GuideOptionResponse> guides = await _guideService
				.SearchSiteChannelsAsync(
					new GuideSiteSearchRequest
					{
						Site = site.Site,
						SearchText = SiteSearchText,
						PageNumber = pageNumber,
						PageSize = SiteChannelPageSize
					},
					cancellationToken)
				.ConfigureAwait(true);

			foreach (GuideOptionResponse guide in guides)
				SiteChannels.Add(guide);

			SiteChannelPageNumber = guides.MetaData.CurrentPage;
			SiteChannelTotalPages = guides.MetaData.TotalPages;
			SiteStatusMessage = Resources.GuideSitePageStatus.FormatMessage(SiteChannelPageNumber, SiteChannelTotalPages, guides.MetaData.TotalCount);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task LoadOptionsAndReportAsync()
	{
		try
		{
			await LoadOptionsAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	private async Task LoadSiteChannelsAndReportAsync(int pageNumber)
	{
		try
		{
			await LoadSiteChannelsAsync(pageNumber).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	private void ApplyOption()
	{
		if (SelectedMapping is not { } mapping || SelectedOption is not { } option)
			return;

		mapping.Apply(option);
		StatusMessage = Resources.GuideMappingStatus.FormatMessage(CompleteCount, Mappings.Count);
	}

	private void ApplySiteChannel()
	{
		if (SelectedMapping is not { } mapping || SelectedSiteChannel is not { } option)
			return;

		mapping.Apply(option);
		StatusMessage = Resources.GuideMappingStatus.FormatMessage(CompleteCount, Mappings.Count);
	}

	/// <summary>
	/// Indicates whether the row already holds what the guide says.
	/// </summary>
	private bool IsApplied(GuideOptionResponse option)
		=> SelectedMapping is { } mapping
		&& string.Equals(option.Site, mapping.Site, StringComparison.OrdinalIgnoreCase)
		&& string.Equals(option.SiteId, mapping.SiteId, StringComparison.OrdinalIgnoreCase);

	private void OnMappingPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(GuideMappingViewModel.IsComplete))
			RaiseCountsChanged();
	}

	private void RaiseCountsChanged()
	{
		RaisePropertyChanged(nameof(CompleteCount));
		RaisePropertyChanged(nameof(HasMappings));
	}

	private void RaiseCommandStatesChanged()
	{
		_saveCommand?.RaiseCanExecuteChanged();
		_exportCommand?.RaiseCanExecuteChanged();
		_applyOptionCommand?.RaiseCanExecuteChanged();
		_applySiteChannelCommand?.RaiseCanExecuteChanged();
		_refreshSitesCommand?.RaiseCanExecuteChanged();
		_searchSiteChannelsCommand?.RaiseCanExecuteChanged();
		_previousSiteChannelPageCommand?.RaiseCanExecuteChanged();
		_nextSiteChannelPageCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.GuideOperationFailed, exception));
}
