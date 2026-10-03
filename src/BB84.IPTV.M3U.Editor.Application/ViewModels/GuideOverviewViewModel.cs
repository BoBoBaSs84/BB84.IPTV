// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Collections.ObjectModel;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Application.Contracts.Requests;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Application.Enumerators;
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
/// Represents the guide overview screen: which providers carry a guide for a channel, searched
/// across every provider and ordered by any of its columns.
/// </summary>
/// <remarks>
/// The screen only reads. The mapping of a playlist stays with <see cref="GuideViewModel"/>.
/// </remarks>
public sealed class GuideOverviewViewModel : ViewModelBase, INavigateable
{
	/// <summary>
	/// The number of guides that are shown at once.
	/// </summary>
	private const int GuidePageSize = Parameters.MinPageSize;

	private readonly IGuideService _guideService;
	private readonly IEventService _eventService;
	private readonly List<GuideSiteResponse> _allProviders = [];
	private CancellationTokenSource? _loadTokenSource;
	private bool _providersLoaded;
	private bool _filteringProviders;
	private string _providerSearchText = string.Empty;
	private GuideSiteResponse? _selectedProvider;
	private string _searchText = string.Empty;
	private GuideSortColumn _sortBy;
	private bool _descending;
	private int _pageNumber = 1;
	private int _totalPages;
	private int _totalCount;
	private bool _isBusy;
	private string _statusMessage = string.Empty;
	private string _providerStatusMessage = string.Empty;
	private AsyncActionCommand? _searchCommand;
	private AsyncActionCommand? _previousPageCommand;
	private AsyncActionCommand? _nextPageCommand;
	private AsyncActionCommand? _refreshProvidersCommand;
	private ActionCommand? _clearProviderCommand;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuideOverviewViewModel"/> class.
	/// </summary>
	/// <param name="guideService">The service that searches the guides and the providers.</param>
	/// <param name="eventService">The service that publishes status and error events.</param>
	public GuideOverviewViewModel(IGuideService guideService, IEventService eventService)
	{
		_guideService = guideService;
		_eventService = eventService;

		// The commands depend on the selection and the rows, the UI only re-queries them when told so.
		PropertyChanged += (s, e) => RaiseCommandStatesChanged();
		Guides.CollectionChanged += (s, e) => RaiseCommandStatesChanged();
	}

	/// <summary>
	/// Gets the providers the program guides are grabbed from, the ones the filter keeps.
	/// </summary>
	public ObservableCollection<GuideSiteResponse> Providers { get; } = [];

	/// <summary>
	/// Gets the guides of the current page, one per channel and provider.
	/// </summary>
	public ObservableCollection<GuideOptionResponse> Guides { get; } = [];

	/// <summary>
	/// Gets or sets the text the providers are filtered by, which is done without the database.
	/// </summary>
	public string ProviderSearchText
	{
		get => _providerSearchText;
		set
		{
			if (SetProperty(ref _providerSearchText, value))
				FilterProviders();
		}
	}

	/// <summary>
	/// Gets or sets the provider the guides are limited to, <see langword="null"/> for every provider.
	/// </summary>
	public GuideSiteResponse? SelectedProvider
	{
		get => _selectedProvider;
		set
		{
			// The filter rebuilds the list, which makes a bound list drop its selection on its own.
			if (_filteringProviders)
				return;

			if (SetProperty(ref _selectedProvider, value))
				_ = LoadAndReportAsync(1);
		}
	}

	/// <summary>
	/// Gets or sets the text a guide must hold, in its channel, provider, identifiers or language.
	/// </summary>
	public string SearchText
	{
		get => _searchText;
		set => SetProperty(ref _searchText, value);
	}

	/// <summary>
	/// Gets the column the guides are ordered by.
	/// </summary>
	public GuideSortColumn SortBy
	{
		get => _sortBy;
		private set => SetProperty(ref _sortBy, value);
	}

	/// <summary>
	/// Indicates whether the guides are ordered the other way round.
	/// </summary>
	public bool Descending
	{
		get => _descending;
		private set => SetProperty(ref _descending, value);
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
	/// Gets the number of pages the guides of the search have.
	/// </summary>
	[NotifyChanged(nameof(HasPreviousPage), nameof(HasNextPage))]
	public int TotalPages
	{
		get => _totalPages;
		private set => SetProperty(ref _totalPages, value);
	}

	/// <summary>
	/// Gets the number of guides the search found.
	/// </summary>
	public int TotalCount
	{
		get => _totalCount;
		private set => SetProperty(ref _totalCount, value);
	}

	/// <summary>
	/// Indicates whether a page of guides comes before the one that is shown.
	/// </summary>
	public bool HasPreviousPage
		=> PageNumber > 1;

	/// <summary>
	/// Indicates whether a page of guides comes after the one that is shown.
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
	/// Gets what the result shows, e.g. which page of how many guides.
	/// </summary>
	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	/// <summary>
	/// Gets what the provider list shows, e.g. how many providers the filter keeps.
	/// </summary>
	public string ProviderStatusMessage
	{
		get => _providerStatusMessage;
		private set => SetProperty(ref _providerStatusMessage, value);
	}

	/// <summary>
	/// Gets the command that searches the guides, from the first page.
	/// </summary>
	public IAsyncActionCommand SearchCommand
		=> _searchCommand ??= new AsyncActionCommand(() => LoadAsync(1), () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that shows the page of guides before the one that is shown.
	/// </summary>
	public IAsyncActionCommand PreviousPageCommand
		=> _previousPageCommand ??= new AsyncActionCommand(() => LoadAsync(PageNumber - 1), () => !IsBusy && HasPreviousPage, OnError);

	/// <summary>
	/// Gets the command that shows the page of guides after the one that is shown.
	/// </summary>
	public IAsyncActionCommand NextPageCommand
		=> _nextPageCommand ??= new AsyncActionCommand(() => LoadAsync(PageNumber + 1), () => !IsBusy && HasNextPage, OnError);

	/// <summary>
	/// Gets the command that reads the providers again, e.g. after the catalog was imported anew.
	/// </summary>
	public IAsyncActionCommand RefreshProvidersCommand
		=> _refreshProvidersCommand ??= new AsyncActionCommand(() => LoadProvidersAsync(true), () => !IsBusy, OnError);

	/// <summary>
	/// Gets the command that drops the provider, so every provider is searched again.
	/// </summary>
	public IActionCommand ClearProviderCommand
		=> _clearProviderCommand ??= new ActionCommand(() => SelectedProvider = null, () => !IsBusy && SelectedProvider is not null);

	/// <summary>
	/// Loads the providers and the first page of guides, and reports a failure instead of throwing.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAndReportAsync()
	{
		try
		{
			await LoadProvidersAsync().ConfigureAwait(true);
			await LoadAsync(1).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Loads the providers the program guides are grabbed from, once per screen unless a fresh read
	/// is asked for.
	/// </summary>
	/// <param name="force">Reads the providers again although they were read before.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadProvidersAsync(bool force = false, CancellationToken cancellationToken = default)
	{
		if (_providersLoaded && !force)
			return;

		IsBusy = true;

		try
		{
			IReadOnlyList<GuideSiteResponse> providers = await _guideService
				.GetSitesAsync(cancellationToken)
				.ConfigureAwait(true);

			_allProviders.Clear();
			_allProviders.AddRange(providers);
			_providersLoaded = true;

			FilterProviders();
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Loads one page of the guides the search finds.
	/// </summary>
	/// <remarks>
	/// A search that is still running is cancelled, so the result of the newest one is shown.
	/// </remarks>
	/// <param name="pageNumber">The page to show, the first page is page one.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task LoadAsync(int pageNumber)
	{
		CancellationTokenSource tokenSource = new();
		CancellationTokenSource? previous = Interlocked.Exchange(ref _loadTokenSource, tokenSource);

		if (previous is not null)
		{
			await previous.CancelAsync().ConfigureAwait(true);
			previous.Dispose();
		}

		IsBusy = true;

		try
		{
			IPagedList<GuideOptionResponse> guides = await _guideService
				.SearchGuidesAsync(
					new GuideSearchRequest
					{
						SearchText = SearchText,
						Site = SelectedProvider?.Site,
						SortBy = SortBy,
						Descending = Descending,
						PageNumber = pageNumber,
						PageSize = GuidePageSize
					},
					tokenSource.Token)
				.ConfigureAwait(true);

			Guides.Clear();
			foreach (GuideOptionResponse guide in guides)
				Guides.Add(guide);

			PageNumber = guides.MetaData.CurrentPage;
			TotalPages = guides.MetaData.TotalPages;
			TotalCount = guides.MetaData.TotalCount;
			StatusMessage = TotalCount is 0
				? Resources.GuideOverviewNoResult
				: Resources.GuideSitePageStatus.FormatMessage(PageNumber, TotalPages, TotalCount);
		}
		catch (OperationCanceledException)
		{
			// A newer search took over, its result is the one that counts.
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>
	/// Orders the guides by a column, which starts again at the first page.
	/// </summary>
	/// <param name="column">The column to order by.</param>
	/// <param name="descending">Orders the guides the other way round.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async Task SortAsync(GuideSortColumn column, bool descending)
	{
		SortBy = column;
		Descending = descending;

		await LoadAsync(1).ConfigureAwait(true);
	}

	/// <summary>
	/// Loads like <see cref="LoadAsync(int)"/> and reports a failure instead of throwing.
	/// </summary>
	private async Task LoadAndReportAsync(int pageNumber)
	{
		try
		{
			await LoadAsync(pageNumber).ConfigureAwait(true);
		}
		catch (Exception exception)
		{
			OnError(exception);
		}
	}

	/// <summary>
	/// Keeps the providers whose name holds the filter.
	/// </summary>
	private void FilterProviders()
	{
		// A rebuild makes a bound list drop its selection, which must not start a search of its own.
		_filteringProviders = true;

		try
		{
			string text = ProviderSearchText.Trim();

			Providers.Clear();
			foreach (GuideSiteResponse provider in _allProviders)
			{
				if (text.Length is 0 || provider.Site.Contains(text, StringComparison.OrdinalIgnoreCase))
					Providers.Add(provider);
			}

			ProviderStatusMessage = Resources.GuideOverviewProvidersStatus.FormatMessage(Providers.Count, _allProviders.Count);
		}
		finally
		{
			_filteringProviders = false;
		}
	}

	private void RaiseCommandStatesChanged()
	{
		_searchCommand?.RaiseCanExecuteChanged();
		_previousPageCommand?.RaiseCanExecuteChanged();
		_nextPageCommand?.RaiseCanExecuteChanged();
		_refreshProvidersCommand?.RaiseCanExecuteChanged();
		_clearProviderCommand?.RaiseCanExecuteChanged();
	}

	private void OnError(Exception exception)
		=> _eventService.Publish(new ErrorOccuredEvent(Resources.GuideOperationFailed, exception));
}
