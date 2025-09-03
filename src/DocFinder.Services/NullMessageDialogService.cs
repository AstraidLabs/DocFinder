namespace DocFinder.Services;

public sealed class NullMessageDialogService : IMessageDialogService
{
    public static readonly NullMessageDialogService Instance = new();
    private NullMessageDialogService() { }
    public void ShowInformation(string message, string title = "DocFinder") { }
    public void ShowWarning(string message, string title = "DocFinder") { }
    public void ShowError(string message, string title = "DocFinder") { }
    public bool ShowConfirmation(string message, string title = "DocFinder") => false;
}
