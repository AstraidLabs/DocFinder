namespace DocFinder.UI.Services;

public interface IMessageDialogService
{
    void ShowInformation(string message, string title = "DocFinder");
    void ShowWarning(string message, string title = "DocFinder");
    void ShowError(string message, string title = "DocFinder");
    bool ShowConfirmation(string message, string title = "DocFinder");
}
