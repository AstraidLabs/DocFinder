using System.Threading.Tasks;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace DocFinder.App.Services;

public sealed class MessageDialogService : IMessageDialogService
{
    public async Task ShowInformation(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await messageBox.ShowDialogAsync();
    }

    public async Task ShowWarning(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await messageBox.ShowDialogAsync();
    }

    public async Task ShowError(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };

        await messageBox.ShowDialogAsync();
    }

    public async Task<bool> ShowConfirmation(string message, string title = "DocFinder")
    {
        var messageBox = new MessageBox
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No"
        };

        var result = await messageBox.ShowDialogAsync();
        return result == MessageBoxResult.Primary;
    }
}
