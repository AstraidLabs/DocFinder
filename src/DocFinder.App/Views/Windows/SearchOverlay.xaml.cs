using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using DocFinder.App.ViewModels;
using DocFinder.Indexing;
using DocFinder.App.Services;

namespace DocFinder.App.Views.Windows;

public partial class SearchOverlay : FluentWindow
{
    private readonly SearchOverlayViewModel _viewModel;
    private readonly IIndexer _indexer;
    private readonly SettingsWindow _settings;
    private readonly IDocumentViewService _documentViewService;
    private readonly IMessageDialogService _dialogs;
    private ResourceDictionary? _themeDictionary;

    public SearchOverlay(SearchOverlayViewModel viewModel, IIndexer indexer, SettingsWindow settings, IDocumentViewService documentViewService, IMessageDialogService dialogs)
    {
        _viewModel = viewModel;
        _indexer = indexer;
        _settings = settings;
        _documentViewService = documentViewService;
        _dialogs = dialogs;

        InitializeComponent();
        ApplyTheme(ApplicationThemeManager.GetAppTheme());
        ApplicationThemeManager.Changed += OnThemeChanged;
        SystemThemeWatcher.Watch(this);
        SizeChanged += SearchOverlay_SizeChanged;

        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        ApplyResponsiveLayout(ActualWidth);
    }

    private void OnThemeChanged(ApplicationTheme newTheme, Color accentColor)
        => ApplyTheme(newTheme);

    private void ApplyTheme(ApplicationTheme theme)
    {
        if (_themeDictionary != null)
            Resources.MergedDictionaries.Remove(_themeDictionary);

        var source = theme == ApplicationTheme.Dark
            ? new Uri("/DocFinder.App;component/Resources/Theme.Dark.xaml", UriKind.Relative)
            : new Uri("/DocFinder.App;component/Resources/Theme.Light.xaml", UriKind.Relative);

        _themeDictionary = new ResourceDictionary { Source = source };
        Resources.MergedDictionaries.Add(_themeDictionary);
    }

    private void SearchOverlay_SizeChanged(object sender, SizeChangedEventArgs e)
        => ApplyResponsiveLayout(e.NewSize.Width);

    private void ApplyResponsiveLayout(double width)
    {
        DetailColumn.Width = width < 700
            ? new GridLength(0)
            : new GridLength(320);
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (MenuButton.ContextMenu != null)
        {
            MenuButton.ContextMenu.PlacementTarget = MenuButton;
            MenuButton.ContextMenu.IsOpen = true;
        }
    }

    private void Menu_NewSearch_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Query = string.Empty;
        QueryTextBox.Focus();
    }

    private void Menu_Settings_Click(object sender, RoutedEventArgs e)
    {
        _settings.Show();
    }

    private void Menu_Protocols_Click(object sender, RoutedEventArgs e)
    {
        var window = new ProtocolWindow();
        window.Show();
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
        while (depObj != null && depObj is not DataGridRow)
            depObj = VisualTreeHelper.GetParent(depObj);
        if (depObj is DataGridRow row)
            row.IsSelected = true;
    }

    private void OpenProtocol_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedDocument == null)
            return;
        var window = new ProtocolWindow(_viewModel.SelectedDocument.Path);
        window.Show();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchOverlayViewModel.SelectedDocument))
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
