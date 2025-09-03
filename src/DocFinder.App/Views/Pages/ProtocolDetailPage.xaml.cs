using DocFinder.Services;
using DocFinder.UI.ViewModels.Entities;
using Wpf.Ui.Abstractions.Controls;

namespace DocFinder.Views.Pages;

public partial class ProtocolDetailPage : INavigableView<ProtocolViewModel>
{
    public ProtocolViewModel ViewModel { get; private set; } = null!;

    public ProtocolDetailPage()
    {
        InitializeComponent();
    }

    public Task OnNavigatedToAsync()
    {
        ViewModel = NavigationState.SelectedProtocol!;
        DataContext = this;
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}

