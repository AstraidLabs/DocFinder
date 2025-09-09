using DocFinder.App.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace DocFinder.App.Views.Pages;

public partial class FilesPage : INavigableView<FilesViewModel>
{
    public FilesViewModel ViewModel { get; }

    public FilesPage(FilesViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}

