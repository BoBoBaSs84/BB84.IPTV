using System.Windows.Controls;

using RESX = BB84.IPTV.M3U.Editor.Properties.Resources;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl : UserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsControl"/> class.
	/// </summary>
	public SettingsControl()
	{
		InitializeComponent();
		InitializeResources();
	}

	private void InitializeResources()
	{
		GeneralGroupBox.Header = RESX.SettingsControl_GeneralGroupBox_Header;
		AutoSaveCheckBox.Content = RESX.SettingsControl_AutoSaveCheckBox_Content;
		AutoSaveIntervalTextBlock.Text = RESX.SettingsControl_AutoSaveIntervalTextBlock_Text;
		EnableLoggingCheckBox.Content = RESX.SettingsControl_EnableLoggingCheckBox_Content;
		LogLevelTextBlock.Text = RESX.SettingsControl_LogLevelTextBlock_Text;
		LanguageTextBlock.Text = RESX.SettingsControl_LanguageTextBlock_Text;

		DatabaseGroupBox.Header = RESX.SettingsControl_DatabaseGroupBox_Header;
		CommandTimeoutTextBlock.Text = RESX.SettingsControl_CommandTimeoutTextBlock_Text;
		MaxBatchSizeTextBlock.Text = RESX.SettingsControl_MaxBatchSizeTextBlock_Text;

		LoadSettingsButton.Content = RESX.SettingsControl_LoadSettingsButton_Content;
		SaveSettingsButton.Content = RESX.SettingsControl_SaveSettingsButton_Content;
	}
}
