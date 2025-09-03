using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace DocFinder.UI.Services;

public sealed class MessageDialogService : IMessageDialogService
{
    public void ShowInformation(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        messageBox.ShowDialogAsync().GetAwaiter().GetResult();
    }

    public void ShowWarning(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        messageBox.ShowDialogAsync().GetAwaiter().GetResult();
    }

    public void ShowError(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        messageBox.ShowDialogAsync().GetAwaiter().GetResult();
    }

    public bool ShowConfirmation(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No"
        };

        var result = messageBox.ShowDialogAsync().GetAwaiter().GetResult();
        return result == MessageBoxResult.Primary;
    }
}
