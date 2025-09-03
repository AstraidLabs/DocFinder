using DocFinder.Services;
using DocFinder.UI.ViewModels.Entities;
using Wpf.Ui.Abstractions.Controls;

namespace DocFinder.Views.Pages;

public partial class FileDetailPage : INavigableView<FileViewModel>
{
    public FileViewModel ViewModel { get; private set; } = null!;

    public FileDetailPage()
    {
        InitializeComponent();
    }

    public Task OnNavigatedToAsync()
    {
        ViewModel = NavigationState.SelectedFile!;
        DataContext = this;
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}

