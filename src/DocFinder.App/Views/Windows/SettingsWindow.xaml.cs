using Wpf.Ui.Controls;
using DocFinder.App.ViewModels;

namespace DocFinder.App.Views.Windows;

public partial class SettingsWindow : MainWindow
{
    public SettingsViewModel ViewModel { get; }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        Closing += (_, e) => { e.Cancel = true; Hide(); };
    }
}
