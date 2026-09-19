using System.Windows.Controls;

using RESX = BB84.IPTV.M3U.Editor.Properties.Resources;

namespace BB84.IPTV.M3U.Editor.Controls;

/// <summary>
/// Interaction logic for DatabaseControl.xaml
/// </summary>
public partial class DatabaseControl : UserControl
{
	/// <summary>
	/// Initializes an instance of the <see cref="DatabaseControl"/> class.
	/// </summary>
	public DatabaseControl()
	{
		InitializeComponent();
		InitializeResources();
	}

	private void InitializeResources()
	{
		DatabaseOperationsGroupBox.Header = RESX.DatabaseControl_DatabaseOperationsGroupBox_Header;
		CreateDatabaseButton.Content = RESX.DatabaseControl_CreateDatabaseButton_Content;
		CheckDatabaseButton.Content = RESX.DatabaseControl_CheckDatabaseButton_Content;
		ImportDatabaseButton.Content = RESX.DatabaseControl_ImportDatabaseButton_Content;
		
		DatabaseStatusGroupBox.Header = RESX.DatabaseControl_DatabaseStatusGroupBox_Header;
		DatabaseCreatedCheckBox.Content = RESX.DatabaseControl_DatabaseCreatedCheckBox_Content;
		DatabaseCreatingCheckBox.Content = RESX.DatabaseControl_DatabaseCreatingCheckBox_Content;
		DatabaseCheckedCheckBox.Content = RESX.DatabaseControl_DatabaseCheckedCheckBox_Content;
		DatabaseCheckingCheckBox.Content = RESX.DatabaseControl_DatabaseCheckingCheckBox_Content;
		DatabaseImportedCheckBox.Content = RESX.DatabaseControl_DatabaseImportedCheckBox_Content;
		DatabaseImportingCheckBox.Content = RESX.DatabaseControl_DatabaseImportingCheckBox_Content;

		ImportProgressGroupBox.Header = RESX.DatabaseControl_ImportProgressGroupBox_Header;
	}
}
