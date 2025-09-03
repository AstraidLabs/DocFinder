using DocFinder.UI.ViewModels.Entities;

namespace DocFinder.App.Services;

/// <summary>
/// Simple shared state for passing selected items between pages.
/// </summary>
public static class NavigationState
{
    public static ProtocolViewModel? SelectedProtocol { get; set; }
    public static FileViewModel? SelectedFile { get; set; }
}

