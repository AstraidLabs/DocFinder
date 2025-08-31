using Wpf.Ui.Controls;
using DocFinder.UI.ViewModels;

namespace DocFinder.UI.Views;

public partial class SettingsWindow : FluentWindow
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }
}
