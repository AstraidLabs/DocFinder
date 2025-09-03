using System.Windows.Controls;
using System.Windows.Input;
using DocFinder.App.Services;
using DocFinder.App.ViewModels.Entities;
using DocFinder.App.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui;

namespace DocFinder.App.Views.Pages;

public partial class ProtocolsPage : INavigableView<ProtocolsViewModel>
{
    private readonly INavigationService _navigationService;
    public ProtocolsViewModel ViewModel { get; }

    public ProtocolsPage(ProtocolsViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        DataContext = this;
        InitializeComponent();
    }

    private void OnProtocolDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem { DataContext: ProtocolViewModel protocol })
        {
            NavigationState.SelectedProtocol = protocol;
            _navigationService.Navigate(typeof(ProtocolDetailPage));
        }
    }
}

