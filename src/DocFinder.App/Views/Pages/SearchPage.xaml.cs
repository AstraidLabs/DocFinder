using DocFinder.App.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace DocFinder.App.Views.Pages;

public partial class SearchPage : INavigableView<SearchViewModel>
{
    public SearchViewModel ViewModel { get; }

    public SearchPage(SearchViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
