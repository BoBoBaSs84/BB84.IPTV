using System.Windows.Controls;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Application.Services;
using BB84.IPTV.M3U.Editor.Application.ViewModels;
using BB84.IPTV.M3U.Editor.Domain.Abstractions.Models;

using RESX = BB84.IPTV.M3U.Editor.Presentation.Properties.Resources;

namespace BB84.IPTV.M3U.Editor.Presentation.Controls;

/// <summary>
/// Represents the playlist control in the IPTV M3U Editor application.
/// </summary>
public partial class PlaylistControl : UserControl
{
	private readonly IProviderService _providerService;
	private readonly PlaylistViewModel _viewModel;

	/// <summary>
	/// Initializes a new instance of the <see cref="PlaylistControl"/> class.
	/// </summary>
	/// <param name="providerService">The provider service instance to use.</param>
	/// <param name="viewModel">The playlist view model instance to use.</param>
	public PlaylistControl(IProviderService providerService, PlaylistViewModel viewModel)
	{
		InitializeComponent();

		_providerService = providerService;
		_viewModel = viewModel;
		DataContext = _viewModel;

		SetToolTips();
	}

	private void EntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (entriesDataGrid.SelectedItem is IEntry entry)
			_viewModel.SelectedEntry = entry;
	}

	/// <inheritdoc/>
	protected override async void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);

		if (!string.IsNullOrWhiteSpace(_viewModel.FilePath) && _providerService.File.Exists(_viewModel.FilePath))
			await _viewModel.LoadPlaylistAsync(_viewModel.FilePath).ConfigureAwait(true);
	}

	private void SetToolTips()
	{
		filePathLabel.ToolTip = RESX.ToolTip_Playlist_FilePath;
		tvGuidePathLabel.ToolTip = RESX.ToolTip_Playlist_TVGuidePath;
		cacheLabel.ToolTip = RESX.ToolTip_Playlist_Cache;
		deinterlaceLabel.ToolTip = RESX.ToolTip_Playlist_Deinterlace;
		refreshLabel.ToolTip = RESX.ToolTip_Playlist_Refresh;
		trackDurationLabel.ToolTip = RESX.ToolTip_Track_Duration;
		trackTitleLabel.ToolTip = RESX.ToolTip_Track_Title;
		trackLocationLabel.ToolTip = RESX.ToolTip_Track_Location;
		trackGroupingLabel.ToolTip = RESX.ToolTip_Track_Grouping;
		metadataNameLabel.ToolTip = RESX.ToolTip_Metadata_Name;
		metadataLogoLabel.ToolTip = RESX.ToolTip_Metadata_Logo;
		metadataGroupIdLabel.ToolTip = RESX.ToolTip_Metadata_GroupId;
		metadataGroupTitleLabel.ToolTip = RESX.ToolTip_Metadata_GroupTitle;
		metadataCensoredCheckBox.ToolTip = RESX.ToolTip_Metadata_Censored;
	}
}
