using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DocFinder.App.Services;
using DocFinder.App.ViewModels.Pages;
using DocFinder.Indexing;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Pages;

public partial class SearchPage : INavigableView<SearchViewModel>
{
    private readonly SearchViewModel _viewModel;
    private readonly IIndexer _indexer;
    private readonly INavigationService _navigationService;
    private readonly IDocumentViewService _documentViewService;
    private readonly IMessageDialogService _dialogs;

    public SearchViewModel ViewModel => _viewModel;

    public SearchPage(
        SearchViewModel viewModel,
        IIndexer indexer,
        INavigationService navigationService,
        IDocumentViewService documentViewService,
        IMessageDialogService dialogs)
    {
        _viewModel = viewModel;
        _indexer = indexer;
        _navigationService = navigationService;
        _documentViewService = documentViewService;
        _dialogs = dialogs;

        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        SizeChanged += SearchPage_SizeChanged;
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

    private void Menu_NewSearch_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Query = string.Empty;
        QueryTextBox.Focus();
    }

    private void Menu_Settings_Click(object sender, RoutedEventArgs e)
    {
        _navigationService.Navigate(typeof(SettingsPage));
    }

    private async void Menu_Reindex_Click(object sender, RoutedEventArgs e)
    {
        if (!_dialogs.ShowConfirmation("Přeindexovat všechny dokumenty?", "DocFinder"))
            return;

        try
        {
            await _indexer.ReindexAllAsync();
            _dialogs.ShowInformation("Přeindexování dokončeno", "DocFinder");
        }
        catch (Exception ex)
        {
            _dialogs.ShowError($"Přeindexování selhalo: {ex.Message}", "DocFinder");
        }
    }

    private void Menu_PauseResume_Click(object sender, RoutedEventArgs e)
    {
        if (_indexer.State == IndexingState.Indexing)
        {
            _indexer.Pause();
            PauseResumeMenuItem.Header = "Pokračovat v indexaci";
            PauseResumeIcon.Symbol = SymbolRegular.PlayCircle24;
        }
        else
        {
            _indexer.Resume();
            PauseResumeMenuItem.Header = "Pozastavit indexaci";
            PauseResumeIcon.Symbol = SymbolRegular.PauseCircle24;
        }
    }

    private void Menu_Exit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
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

    private void ResultsGrid_ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.ContextMenu menu)
            return;

        var items = menu.Items.OfType<System.Windows.Controls.MenuItem>().ToList();
        var openDetail = items.FirstOrDefault(i => i.Header?.ToString() == "Otevřít detail souboru");

        var doc = _viewModel.SelectedDocument;
        var hasDoc = doc != null;

        if (openDetail != null)
            openDetail.IsEnabled = hasDoc;
    }

    private void OpenFileDetail_Click(object sender, RoutedEventArgs e)
    {
        var doc = _viewModel.SelectedDocument;
        if (doc == null)
            return;

        var info = new FileInfo(doc.Path);
        var detail = $"Název: {info.Name}\nTyp: {info.Extension}\nVelikost: {info.Length} B\nZměněno: {info.LastWriteTime}";
        _dialogs.ShowInformation(detail, "Detail souboru");
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
}

