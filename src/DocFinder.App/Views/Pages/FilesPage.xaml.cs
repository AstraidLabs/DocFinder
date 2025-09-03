using System.Windows.Controls;
using System.Windows.Input;
using DocFinder.App.Services;
using DocFinder.UI.ViewModels.Entities;
using DocFinder.App.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui;

namespace DocFinder.App.Views.Pages;

public partial class FilesPage : INavigableView<FilesViewModel>
{
    private readonly INavigationService _navigationService;
    public FilesViewModel ViewModel { get; }

    public FilesPage(FilesViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        DataContext = this;
        InitializeComponent();
    }

    private void OnFileDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem { DataContext: FileViewModel file })
        {
            NavigationState.SelectedFile = file;
            _navigationService.Navigate(typeof(FileDetailPage));
        }
    }
}

