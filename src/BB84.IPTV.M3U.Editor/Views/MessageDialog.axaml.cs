using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

using BB84.IPTV.M3U.Editor.Application.Enumerators;

using RESX = BB84.IPTV.M3U.Editor.Properties.Resources;

namespace BB84.IPTV.M3U.Editor.Views;

/// <summary>
/// The kind of a <see cref="MessageDialog"/>, which selects its icon.
/// </summary>
internal enum MessageDialogKind
{
	/// <summary>
	/// An error message.
	/// </summary>
	Error,

	/// <summary>
	/// An informational message.
	/// </summary>
	Information,

	/// <summary>
	/// A warning message.
	/// </summary>
	Warning,

	/// <summary>
	/// A question to the user.
	/// </summary>
	Question
}

/// <summary>
/// A modal message box, the Avalonia replacement for the WPF <c>MessageBox</c>.
/// </summary>
public partial class MessageDialog : Window
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MessageDialog"/> class.
	/// </summary>
	public MessageDialog()
		=> InitializeComponent();

	/// <summary>
	/// Initializes a new instance of the <see cref="MessageDialog"/> class.
	/// </summary>
	/// <param name="message">The message to show.</param>
	/// <param name="caption">The window title.</param>
	/// <param name="kind">The kind of the message, which selects the icon.</param>
	/// <param name="buttons">The buttons to show, the first one is the default button.</param>
	internal MessageDialog(string message, string caption, MessageDialogKind kind, params NotificationResult[] buttons) : this()
	{
		Title = caption;
		MessageText.Text = message;

		(IconBorder.Background, IconText.Text) = kind switch
		{
			MessageDialogKind.Error => (Brushes.Firebrick, "✕"),
			MessageDialogKind.Warning => (Brushes.DarkOrange, "!"),
			MessageDialogKind.Question => (Brushes.SteelBlue, "?"),
			_ => (Brushes.SteelBlue, "i")
		};

		for (int i = 0; i < buttons.Length; i++)
			ButtonPanel.Children.Add(CreateButton(buttons[i], isDefault: i == 0));
	}

	/// <summary>
	/// Gets the button the user clicked, <see cref="NotificationResult.None"/> if the dialog was closed otherwise.
	/// </summary>
	internal NotificationResult Result { get; private set; }

	private Button CreateButton(NotificationResult result, bool isDefault)
	{
		Button button = new()
		{
			Content = GetButtonText(result),
			MinWidth = 80,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			IsDefault = isDefault,
			IsCancel = result is NotificationResult.Cancel or NotificationResult.No
		};

		button.Click += (s, e) =>
		{
			Result = result;
			Close();
		};

		return button;
	}

	private static string GetButtonText(NotificationResult result) => result switch
	{
		NotificationResult.Cancel => RESX.MessageDialog_CancelButton_Content,
		NotificationResult.Yes => RESX.MessageDialog_YesButton_Content,
		NotificationResult.No => RESX.MessageDialog_NoButton_Content,
		_ => RESX.MessageDialog_OkButton_Content
	};
}