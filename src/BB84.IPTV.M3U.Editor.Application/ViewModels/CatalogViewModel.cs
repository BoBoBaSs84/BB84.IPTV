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
using BB84.IPTV.M3U.Editor.Application.Enumerators;
using BB84.IPTV.M3U.Editor.Application.Events;
using BB84.IPTV.M3U.Editor.Application.Extensions;
using BB84.IPTV.M3U.Editor.Application.Features;
using BB84.IPTV.M3U.Editor.Application.Properties;
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;
using BB84.IPTV.M3U.Editor.Domain.Models;
using BB84.Notifications.Attributes;
using BB84.Notifications.Commands;
using BB84.Notifications.Interfaces.Commands;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels;

/// <summary>
/// Represents the channel screen: the searchable iptv-org catalog and the user defined channels.
/// </summary>
/// <remarks>
/// Channels are added to the playlist that is open in the editor, the editor itself stays on the
/// playlist screen, so the added entries are saved with the playlist as usual.
/// </remarks>
public sealed class CatalogViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// The number of channels per page, a compromise between the number of pages and the time one page takes.
	/// </summary>
	public const int PageSize = 500;

	/// <summary>
	/// The number of user defined channels per page.
	/// </summary>
	public const int CustomChannelPageSize = Parameters.MaxPageSize;

	private readonly ICatalogService _catalogService;
	private readonly ICustomChannelService _customChannelService;
	private readonly INotificationService _notificationService;
	private readonly IEventService _eventService;
	private string _searchText = string.Empty;
	private CatalogFilterValue? _selectedCountry;
	private CatalogFilterValue? _selectedLanguage;
	private CatalogFilterValue? _selectedCategory;
	private bool _includeNsfw;
	private bool _includeWithoutStream;
	private bool _isBusy;
	private bool _filtersLoaded;
	private string _resultMessage = string.Empty;
	private int _pageNumber = 1;
	private int _totalPages;
	private int _totalCount;
	private int _customChannelPageNumber = 1;
	private int _customChannelTotalPages;
	private CatalogChannelViewModel? _selectedChannel;
	private CustomChannelViewModel? _selectedCustomChannel;
	private AsyncActionCommand? _searchCommand;
	private AsyncActionCommand? _previousPageCommand;
	private AsyncActionCommand? _nextPageCommand;
	private ActionCommand? _resetFiltersCommand;
	private ActionCommand? _addChannelCommand;
	private ActionCommand? _addAllChannelsCommand;
	private ActionCommand? _newCustomChannelCommand;
	private AsyncActionCommand? _saveCustomChannelCommand;
	private AsyncActionCommand? _deleteCustomChannelCommand;
	private ActionCommand? _addCustomChannelCommand;
	private AsyncActionCommand? _previousCustomChannelPageCommand;
	private AsyncActionCommand? _nextCustomChannelPageCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="CatalogViewModel"/> class.
	/// </summary>
	/// <param name="catalogService">The service that searches the imported catalog.</param>
	/// <param name="customChannelService">The service that stores the user defined channels.</param>
	/// <param name="notificationService">The service that shows messages and questions.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	/// <param name="editor">The editor of the open playlist, the channels are added to it.</param>
	public CatalogViewModel(ICatalogService catalogService, ICustomChannelService customChannelService, INotificationService notificationService, IEventService eventService, PlaylistViewModel editor)
	{
		_catalogService = catalogService;
		_customChannelService = customChannelService;
		_notificationService = notificationService;
		_eventService = eventService;
		Editor = editor;

		// The commands depend on the editor, the selection and the result, the UI only re-queries them when told so.
		Editor.PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Channels.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the editor of the open playlist.
	/// </summary>
	public PlaylistViewModel Editor { get; }

	/// <summary>
	/// Gets the countries a search can be filtered by.
	/// </summary>
	public ObservableCollection<CatalogFilterValue> Countries { get; } = [];

	/// <summary>
	/// Gets the languages a search can be filtered by.
	/// </summary>
	public ObservableCollection<CatalogFilterValue> Languages { get; } = [];

	/// <summary>
	/// Gets the categories a search can be filtered by.
	/// </summary>
	public ObservableCollection<CatalogFilterValue> Categories { get; } = [];

	/// <summary>
	/// Gets the channels found by the last search.
	/// </summary>
	public ObservableCollection<CatalogChannelViewModel> Channels { get; } = [];

	/// <summary>
	/// Gets the stored user defined channels.
	/// </summary>
	public ObservableCollection<CustomChannelViewModel> CustomChannels { get; } = [];

	/// <summary>
	/// Gets or sets the text the channel name or the iptv-org identifier must contain.
	/// </summary>
	public string SearchText
	{
		get => _searchText;
		set => SetProperty(ref _searchText, value);
	}

	/// <summary>
	/// Gets or sets the country the channels must be based in, <see langword="null"/> for all countries.
	/// </summary>
	public CatalogFilterValue? SelectedCountry
	{
		get => _selectedCountry;
		set => SetProperty(ref _selectedCountry, value);
	}

	/// <summary>
	/// Gets or sets the language the channels must broadcast in, <see langword="null"/> for all languages.
	/// </summary>
	public CatalogFilterValue? SelectedLanguage
	{
		get => _selectedLanguage;
		set => SetProperty(ref _selectedLanguage, value);
	}

	/// <summary>
	/// Gets or sets the category the channels must belong to, <see langword="null"/> for all categories.
	/// </summary>
	public CatalogFilterValue? SelectedCategory
	{
		get => _selectedCategory;
		set => SetProperty(ref _selectedCategory, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether channels marked as NSFW are part of the result.
	/// </summary>
	public bool IncludeNsfw
	{
		get => _includeNsfw;
		set => SetProperty(ref _includeNsfw, value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether channels without a stream are part of the result.
	/// </summary>
	public bool IncludeWithoutStream
	{
		get => _includeWithoutStream;
		set => SetProperty(ref _includeWithoutStream, value);
	}

	/// <summary>
	/// Gets the result of the last search, e.g. the page and the number of channels found.
	/// </summary>
	public string ResultMessage
	{
		get => _resultMessage;
		private set => SetProperty(ref _resultMessage, value);
	}

	/// <summary>
	/// Gets the number of the page that is shown, the first page is page one.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousPage), nameof(HasNextPage))]
	public int PageNumber
	{
		get => _pageNumber;
		private set => SetProperty(ref _pageNumber, value);
	}

	/// <summary>
	/// Gets the number of pages the result has.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousPage), nameof(HasNextPage))]
	public int TotalPages
	{
		get => _totalPages;
		private set => SetProperty(ref _totalPages, value);
	}

	/// <summary>
	/// Gets the number of channels the search found, not only the ones on the page.
	/// </summary>
	public int TotalCount
	{
		get => _totalCount;
		private set => SetProperty(ref _totalCount, value);
	}

	/// <summary>
	/// Indicates whether a page before the shown one exists.
	/// </summary>
	public bool HasPreviousPage
		=> PageNumber > 1;

	/// <summary>
	/// Indicates whether a page after the shown one exists.
	/// </summary>
	public bool HasNextPage
		=> PageNumber < TotalPages;

	/// <summary>
	/// Indicates whether the view model is busy performing a long-running operation.
	/// </summary>
	public bool IsBusy
	{
		get => _isBusy;
		private set => SetProperty(ref _isBusy, value);
	}

	/// <summary>
	/// Gets or sets the selected catalog channel.
	/// </summary>
	public CatalogChannelViewModel? SelectedChannel
	{
		get => _selectedChannel;
		set => SetProperty(ref _selectedChannel, value);
	}

	/// <summary>
	/// Indicates whether a custom channel is selected, so the detail panel is shown.
	/// </summary>
	public bool SelectedCustomChannelVisible
		=> SelectedCustomChannel is not null;

	/// <summary>
	/// Gets or sets the selected user defined channel.
	/// </summary>
	[NotifyChanged(nameof(SelectedCustomChannelVisible))]
	public CustomChannelViewModel? SelectedCustomChannel
	{
		get => _selectedCustomChannel;
		set
		{
			if (_selectedCustomChannel is not null)
				_selectedCustomChannel.PropertyChanged -= OnCustomChannelPropertyChanged;

			if (SetProperty(ref _selectedCustomChannel, value) && value is not null)
				value.PropertyChanged += OnCustomChannelPropertyChanged;
		}
	}

	/// <summary>
	/// Gets the command that searches the catalog with the current filter.
	/// </summary>
	public IAsyncActionCommand SearchCommand
		=> _searchCommand ??= new AsyncActionCommand(SearchAsync, () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that shows the page before the shown one.
	/// </summary>
	public IAsyncActionCommand PreviousPageCommand
		=> _previousPageCommand ??= new AsyncActionCommand(() => LoadPageAsync(PageNumber - 1), () => !IsBusy && HasPreviousPage, OnError);

	/// <summary>
	/// Gets the command that shows the page after the shown one.
	/// </summary>
	public IAsyncActionCommand NextPageCommand
		=> _nextPageCommand ??= new AsyncActionCommand(() => LoadPageAsync(PageNumber + 1), () => !IsBusy && HasNextPage, OnError);

	/// <summary>
	/// Gets the command that clears the filter and the result.
	/// </summary>
	public IActionCommand ResetFiltersCommand
		=> _resetFiltersCommand ??= new ActionCommand(ResetFilters, () => !IsBusy);

	/// <summary>
	/// Gets the command that adds the selected catalog channel to the open playlist.
	/// </summary>
	public IActionCommand AddChannelCommand
		=> _addChannelCommand ??= new ActionCommand(AddSelectedChannel, () => CanAddToPlaylist && SelectedChannel is not null);

	/// <summary>
	/// Gets the command that adds all found catalog channels to the open playlist.
	/// </summary>
	public IActionCommand AddAllChannelsCommand
		=> _addAllChannelsCommand ??= new ActionCommand(AddAllChannels, () => CanAddToPlaylist && Channels.Count > 0);

	/// <summary>
	/// Gets the command that starts a new, unsaved custom channel.
	/// </summary>
	public IActionCommand NewCustomChannelCommand
		=> _newCustomChannelCommand ??= new ActionCommand(NewCustomChannel, () => !IsBusy);

	/// <summary>
	/// Gets the command that stores the selected custom channel.
	/// </summary>
	public IAsyncActionCommand SaveCustomChannelCommand
		=> _saveCustomChannelCommand ??= new AsyncActionCommand(SaveCustomChannelAsync, () => !IsBusy && SelectedCustomChannel?.IsValid is true, OnError);

	/// <summary>
	/// Gets the command that deletes the selected custom channel after a confirmation.
	/// </summary>
	public IAsyncActionCommand DeleteCustomChannelCommand
		=> _deleteCustomChannelCommand ??= new AsyncActionCommand(DeleteCustomChannelAsync, () => !IsBusy && SelectedCustomChannel is not null, OnError);

	/// <summary>
	/// Gets the command that adds the selected custom channel to the open playlist.
	/// </summary>
	public IActionCommand AddCustomChannelCommand
		=> _addCustomChannelCommand ??= new ActionCommand(AddSelectedCustomChannel, () => CanAddToPlaylist && SelectedCustomChannel?.IsValid is true);

	/// <summary>
	/// Gets the number of the custom channel page that is shown, the first page is page one.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousCustomChannelPage), nameof(HasNextCustomChannelPage), nameof(HasMultipleCustomChannelPages), nameof(CustomChannelPageStatus))]
	public int CustomChannelPageNumber
	{
		get => _customChannelPageNumber;
		private set => SetProperty(ref _customChannelPageNumber, value);
	}

	/// <summary>
	/// Gets the number of pages the stored custom channels fill.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousCustomChannelPage), nameof(HasNextCustomChannelPage), nameof(HasMultipleCustomChannelPages), nameof(CustomChannelPageStatus))]
	public int CustomChannelTotalPages
	{
		get => _customChannelTotalPages;
		private set => SetProperty(ref _customChannelTotalPages, value);
	}

	/// <summary>
	/// Indicates whether a custom channel page before the shown one exists.
	/// </summary>
	public bool HasPreviousCustomChannelPage
		=> CustomChannelPageNumber > 1;

	/// <summary>
	/// Indicates whether a custom channel page after the shown one exists.
	/// </summary>
	public bool HasNextCustomChannelPage
		=> CustomChannelPageNumber < CustomChannelTotalPages;

	/// <summary>
	/// Indicates whether the custom channels fill more than one page, the paging is hidden otherwise.
	/// </summary>
	public bool HasMultipleCustomChannelPages
		=> CustomChannelTotalPages > 1;

	/// <summary>
	/// Gets the status of the custom channel paging, e.g. "Page 1 of 3".
	/// </summary>
	public string CustomChannelPageStatus
		=> Resources.PageStatus.FormatMessage(CustomChannelPageNumber, CustomChannelTotalPages);

	/// <summary>
	/// Gets the command that shows the custom channel page before the shown one.
	/// </summary>
	public IAsyncActionCommand PreviousCustomChannelPageCommand
		=> _previousCustomChannelPageCommand ??= new AsyncActionCommand(() => LoadCustomChannelPageAsync(CustomChannelPageNumber - 1), () => !IsBusy && HasPreviousCustomChannelPage, OnError);

	/// <summary>
	/// Gets the command that shows the custom channel page after the shown one.
	/// </summary>
	public IAsyncActionCommand NextCustomChannelPageCommand
		=> _nextCustomChannelPageCommand ??= new AsyncActionCommand(() => LoadCustomChannelPageAsync(CustomChannelPageNumber + 1), () => !IsBusy && HasNextCustomChannelPage, OnError);

	/// <summary>
	/// Loads like <see cref="LoadAsync"/> and reports a failure instead of throwing.
	/// </summary>
	/// <remarks>
	/// For callers that cannot await, e.g. a view that is shown; a failure would be lost otherwise,
	/// leaving an empty screen without a reason.
	/// </remarks>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAndReportAsync()
	{
		try
		{
			await LoadAsync().ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Loads the filter values and the stored custom channels, the filter values only once.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAsync(CancellationToken cancellationToken = default)
	{
		IsBusy = true;

		try
		{
			if (!_filtersLoaded)
			{
				CatalogFilterResponse filters = await _catalogService
					.GetFiltersAsync(cancellationToken)
					.ConfigureAwait(true);

				Fill(Countries, filters.Countries);
				Fill(Languages, filters.Languages);
				Fill(Categories, filters.Categories);

				// An empty catalog is imported later, so the values are loaded again on the next visit.
				_filtersLoaded = Countries.Count > 0 || Languages.Count > 0 || Categories.Count > 0;
			}

			await LoadCustomChannelsAsync(cancellationToken).ConfigureAwait(true);
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Searches with the current filter, starting again at the first page.
	/// </summary>
	private async Task SearchAsync()
		=> await LoadPageAsync(1).ConfigureAwait(true);

	private async Task LoadPageAsync(int pageNumber)
	{
		IsBusy = true;

		try
		{
			CatalogSearchRequest request = new()
			{
				SearchText = SearchText,
				Country = SelectedCountry?.Code,
				Language = SelectedLanguage?.Code,
				Category = SelectedCategory?.Code,
				IncludeNsfw = IncludeNsfw,
				IncludeWithoutStream = IncludeWithoutStream,
				PageNumber = pageNumber,
				PageSize = PageSize
			};

			IPagedList<CatalogChannelResponse> channels = await _catalogService
				.SearchAsync(request)
				.ConfigureAwait(true);

			Channels.Clear();
			foreach (CatalogChannelResponse channel in channels)
				Channels.Add(new CatalogChannelViewModel(channel));

			SelectedChannel = Channels.FirstOrDefault();
			PageNumber = channels.MetaData.CurrentPage;
			TotalPages = channels.MetaData.TotalPages;
			TotalCount = channels.MetaData.TotalCount;
			ResultMessage = TotalCount > 0
				? Resources.CatalogPageStatus.FormatMessage(PageNumber, TotalPages, TotalCount)
				: Resources.CatalogChannelsFound.FormatMessage(0);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private void ResetFilters()
	{
		SearchText = string.Empty;
		SelectedCountry = null;
		SelectedLanguage = null;
		SelectedCategory = null;
		IncludeNsfw = false;
		IncludeWithoutStream = false;
		Channels.Clear();
		SelectedChannel = null;
		ResultMessage = string.Empty;
		PageNumber = 1;
		TotalPages = 0;
		TotalCount = 0;
	}

	private void AddSelectedChannel()
	{
		if (SelectedChannel is { } channel)
			AddEntries([channel.Channel.ToEntry()]);
	}

	private void AddAllChannels()
		=> AddEntries([.. Channels.Select(channel => channel.Channel.ToEntry())]);

	private void AddSelectedCustomChannel()
	{
		if (SelectedCustomChannel is { IsValid: true } channel)
			AddEntries([channel.ToResponse().ToEntry()]);
	}

	private void AddEntries(IReadOnlyList<EntryModel> entries)
	{
		if (!CanAddToPlaylist)
		{
			ResultMessage = Resources.CatalogNoPlaylistOpen;
			return;
		}

		int added = Editor.AddEntries(entries);
		if (added > 0)
			PublishStatus(Resources.CatalogChannelsAdded.FormatMessage(added, Editor.Name));
	}

	private void NewCustomChannel()
	{
		CustomChannelViewModel channel = new(Resources.NewCustomChannelName);
		CustomChannels.Add(channel);
		SelectedCustomChannel = channel;
	}

	private async Task SaveCustomChannelAsync()
	{
		if (SelectedCustomChannel is not { IsValid: true } channel)
			return;

		IsBusy = true;

		try
		{
			if (channel.IsNew)
			{
				int id = await _customChannelService
					.CreateAsync(channel.ToResponse())
					.ConfigureAwait(true);

				channel.MarkStored(id);

				// Reloaded, so the stored order and the page counters hold the new channel.
				await LoadCustomChannelPageAsync(CustomChannelPageNumber).ConfigureAwait(true);
			}
			else if (!await _customChannelService.UpdateAsync(channel.ToResponse()).ConfigureAwait(true))
			{
				// Deleted elsewhere in the meantime.
				CustomChannels.Remove(channel);
				SelectedCustomChannel = null;
				return;
			}

			PublishStatus(Resources.CustomChannelSaved.FormatMessage(channel.Name));
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task DeleteCustomChannelAsync()
	{
		if (SelectedCustomChannel is not { } channel)
			return;

		if (!channel.IsNew)
		{
			NotificationResult result = await _notificationService
				.ShowQuestionAsync(Resources.DeleteCustomChannelQuestion.FormatMessage(channel.Name))
				.ConfigureAwait(true);

			if (result is not NotificationResult.Yes)
				return;

			IsBusy = true;

			try
			{
				_ = await _customChannelService
					.DeleteAsync(channel.Id)
					.ConfigureAwait(true);
			}
			finally
			{
				IsBusy = false;
			}
		}

		_ = CustomChannels.Remove(channel);
		SelectedCustomChannel = CustomChannels.FirstOrDefault();

		if (channel.IsNew)
			return;

		// The page may hold one channel less than before, and one page less as well.
		if (CustomChannelPageNumber > 1 && CustomChannels.Count is 0)
			CustomChannelPageNumber--;

		await LoadCustomChannelPageAsync(CustomChannelPageNumber).ConfigureAwait(true);
	}

	private async Task LoadCustomChannelsAsync(CancellationToken cancellationToken)
		=> await LoadCustomChannelPageAsync(CustomChannelPageNumber, cancellationToken).ConfigureAwait(true);

	private async Task LoadCustomChannelPageAsync(int pageNumber, CancellationToken cancellationToken = default)
	{
		IPagedList<CustomChannelResponse> channels = await _customChannelService
			.GetChannelsAsync(new CustomChannelSearchRequest { PageNumber = pageNumber, PageSize = CustomChannelPageSize }, cancellationToken)
			.ConfigureAwait(true);

		int? selectedId = SelectedCustomChannel?.Id;
		CustomChannels.Clear();
		foreach (CustomChannelResponse channel in channels)
			CustomChannels.Add(new CustomChannelViewModel(channel));

		CustomChannelPageNumber = channels.MetaData.CurrentPage;
		CustomChannelTotalPages = channels.MetaData.TotalPages;

		// The channel that was selected before stays selected, otherwise the first one is shown.
		SelectedCustomChannel = CustomChannels.FirstOrDefault(channel => channel.Id == selectedId)
			?? CustomChannels.FirstOrDefault();
	}

	private bool CanAddToPlaylist
		=> Editor.HasPlaylist && !Editor.IsBusy && !IsBusy;

	private static void Fill(ObservableCollection<CatalogFilterValue> target, IReadOnlyList<CatalogFilterValue> values)
	{
		target.Clear();
		foreach (CatalogFilterValue value in values)
			target.Add(value);
	}

	private void OnCustomChannelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		=> RaiseCommandStatesChanged();

	private void RaiseCommandStatesChanged()
	{
		_searchCommand?.RaiseCanExecuteChanged();
		_previousPageCommand?.RaiseCanExecuteChanged();
		_nextPageCommand?.RaiseCanExecuteChanged();
		_resetFiltersCommand?.RaiseCanExecuteChanged();
		_addChannelCommand?.RaiseCanExecuteChanged();
		_addAllChannelsCommand?.RaiseCanExecuteChanged();
		_newCustomChannelCommand?.RaiseCanExecuteChanged();
		_saveCustomChannelCommand?.RaiseCanExecuteChanged();
		_deleteCustomChannelCommand?.RaiseCanExecuteChanged();
		_addCustomChannelCommand?.RaiseCanExecuteChanged();
		_previousCustomChannelPageCommand?.RaiseCanExecuteChanged();
		_nextCustomChannelPageCommand?.RaiseCanExecuteChanged();
	}

	private void PublishStatus(string message)
		=> _eventService.Publish(new DelayedStatusChangedEvent(message));

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.CatalogOperationFailed, exception));
}