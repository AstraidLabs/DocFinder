using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DocFinder.App.Services;
using DocFinder.App.ViewModels.Pages;
using Wpf.Ui.Controls;
using Wpf.Ui.Abstractions.Controls;

namespace DocFinder.App.Views.Pages;

public partial class SearchPage : INavigableView<SearchViewModel>
{
    private readonly SearchViewModel _viewModel;
    private readonly IDocumentViewService _documentViewService;

    public SearchViewModel ViewModel => _viewModel;

    public SearchPage(SearchViewModel viewModel,
        IDocumentViewService documentViewService)
    {
        _viewModel = viewModel;
        _documentViewService = documentViewService;

        InitializeComponent();
        DataContext = _viewModel;
        SizeChanged += SearchPage_SizeChanged;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        Unloaded += SearchPage_Unloaded;
        ApplyResponsiveLayout(ActualWidth);
    }

    private void SearchPage_SizeChanged(object sender, SizeChangedEventArgs e)
        => ApplyResponsiveLayout(e.NewSize.Width);

    private void ApplyResponsiveLayout(double width)
    {
        DetailColumn.Width = width < 700
            ? new GridLength(0)
            : new GridLength(320);
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    private void ResultsGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var depObj = e.OriginalSource as DependencyObject;
        while (depObj != null && depObj is not System.Windows.Controls.DataGridRow)
            depObj = VisualTreeHelper.GetParent(depObj);
        if (depObj is System.Windows.Controls.DataGridRow row)
            row.IsSelected = true;
        else if (sender is System.Windows.Controls.DataGrid grid)
            grid.UnselectAll();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchViewModel.SelectedDocument))
        {
            UpdatePreview();
        }
    }

    private void UpdatePreview()
    {
        PreviewHost.Content = _viewModel.SelectedDocument != null
            ? _documentViewService.GetViewer(_viewModel.SelectedDocument)
            : null;
    }

    private void ResultsGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.DataGrid dg)
        {
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            dg.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }

    private void SearchPage_Unloaded(object? sender, RoutedEventArgs e)
    {
        SizeChanged -= SearchPage_SizeChanged;
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        Unloaded -= SearchPage_Unloaded;

        ResultsGrid.Loaded -= ResultsGrid_Loaded;
        ResultsGrid.PreviewMouseRightButtonDown -= ResultsGrid_PreviewMouseRightButtonDown;

        ResultsGrid.BeginAnimation(UIElement.OpacityProperty, null);
        PreviewHost.Content = null;
    }
}
