using System;
using System.Windows;
using Wpf.Ui.Controls;
using DocFinder.UI.ViewModels;
using DocFinder.Indexing;
using DocFinder.Services;

namespace DocFinder.UI.Views;

public partial class SearchOverlay : FluentWindow
{
    private readonly SearchOverlayViewModel _viewModel;
    private readonly IIndexer _indexer;
    private readonly ITrayService _tray;
    private readonly SettingsWindow _settings;

    public SearchOverlay(SearchOverlayViewModel viewModel, IIndexer indexer, ITrayService tray, SettingsWindow settings)
    {
        _viewModel = viewModel;
        _indexer = indexer;
        _tray = tray;
        _settings = settings;

        InitializeComponent();
        DataContext = _viewModel;
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
        _settings.Owner = this;
        _settings.Show();
        _settings.Activate();
    }

    private async void Menu_Reindex_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _indexer.ReindexAllAsync();
            _tray.ShowNotification("DocFinder", "Přeindexování dokončeno");
        }
        catch (Exception ex)
        {
            _tray.ShowNotification("DocFinder", $"Přeindexování selhalo: {ex.Message}");
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
        Application.Current.Shutdown();
    }
}
