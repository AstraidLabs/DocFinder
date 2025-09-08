using System.Threading.Tasks;

namespace DocFinder.App.Services;

public interface IMessageDialogService
{
    Task ShowInformation(string message, string title = "DocFinder");
    Task ShowWarning(string message, string title = "DocFinder");
    Task ShowError(string message, string title = "DocFinder");
    Task<bool> ShowConfirmation(string message, string title = "DocFinder");
}
