using System.Windows;

namespace DocFinder.UI.Services;

public sealed class MessageDialogService : IMessageDialogService
{
    public void ShowInformation(string message, string title = "DocFinder")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "DocFinder")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, string title = "DocFinder")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool ShowConfirmation(string message, string title = "DocFinder")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
