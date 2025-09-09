using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DocFinder.App.Services;
using DocFinder.Application;
using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Indexing;
using DocFinder.App.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using System.IO;
using IOFile = System.IO.File;

namespace DocFinder.App.ViewModels.Pages;

public partial class SearchViewModel : ObservableObject
{
    private readonly CommandDispatcher _dispatcher;
    private readonly ISettingsService _settings;
    private readonly IIndexer _indexer;
    private readonly IMessageDialogService _dialogs;
    private readonly INavigationService _navigation;

    private CancellationTokenSource _cts = new();
    private Task? _currentQuery;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private string _fileTypeFilter = "all";

    public ObservableCollection<SearchHit> Results { get; } = new();

    public CollectionViewSource ResultsView { get; } = new();

    [ObservableProperty]
    private SearchHit? _selectedDocument;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private string _authorFilter = string.Empty;

    [ObservableProperty]
    private string _versionFilter = string.Empty;

    [ObservableProperty]
    private string _sortField = nameof(SearchHit.FileName);

    [ObservableProperty]
    private bool _sortAscending = true;

    [ObservableProperty]
    private bool _isIndexing;

    public string PauseResumeText => IsIndexing ? "Pozastavit indexaci" : "Pokračovat v indexaci";

    public SymbolRegular PauseResumeSymbol => IsIndexing ? SymbolRegular.PauseCircle24 : SymbolRegular.PlayCircle24;

    public SearchViewModel(CommandDispatcher dispatcher,
        ISettingsService settings,
        IIndexer indexer,
        IMessageDialogService dialogs,
        INavigationService navigation)
    {
        _dispatcher = dispatcher;
        _settings = settings;
        _indexer = indexer;
        _dialogs = dialogs;
        _navigation = navigation;
        _isIndexing = _indexer.State == IndexingState.Indexing;

        ResultsView.Source = Results;
        UpdateSort();
        _currentQuery = RunQueryAsync(string.Empty, _cts.Token);
    }

    private void RestartQuery(string? overrideQuery = null)
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _currentQuery = RunQueryAsync(overrideQuery ?? Query, _cts.Token);
    }

    partial void OnQueryChanged(string value) => RestartQuery(value);

    partial void OnFileTypeFilterChanged(string value) => RestartQuery();

    private async Task RunQueryAsync(string value, CancellationToken ct)
    {
        try
        {
            var filter = new SearchFilter(
                FileTypeFilter,
                string.IsNullOrWhiteSpace(AuthorFilter) ? null : AuthorFilter,
                string.IsNullOrWhiteSpace(VersionFilter) ? null : VersionFilter,
                FromDate,
                ToDate);

            var command = new SearchDocumentsCommand(value, filter);

            var result = await _dispatcher.SendAsync<SearchDocumentsCommand, SearchResult>(command, ct);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Results.Clear();
                foreach (var hit in result.Hits)
                    Results.Add(hit);
                ResultsView.View.Refresh();
            });
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private Task Search()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _currentQuery = RunQueryAsync(Query, _cts.Token);
        return _currentQuery;
    }

    [RelayCommand(CanExecute = nameof(CanOpenDocument))]
    private void OpenDocument()
    {
        if (SelectedDocument == null)
            return;

        var path = SelectedDocument.Path;
        if (!IOFile.Exists(path))
            return;
        if (!_settings.Current.WatchedRoots.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            return;
        var psi = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
        System.Diagnostics.Process.Start(psi);
    }

    private bool CanOpenDocument() => SelectedDocument != null;

    [RelayCommand]
    private void NewSearch() => Query = string.Empty;

    [RelayCommand]
    private async Task ReindexAsync()
    {
        if (!await _dialogs.ShowConfirmation("Přeindexovat všechny dokumenty?", "DocFinder"))
            return;

        try
        {
            await _indexer.ReindexAllAsync();
            await _dialogs.ShowInformation("Přeindexování dokončeno", "DocFinder");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowError($"Přeindexování selhalo: {ex.Message}", "DocFinder");
        }
    }

    [RelayCommand]
    private void ToggleIndexing()
    {
        if (IsIndexing)
        {
            _indexer.Pause();
            IsIndexing = false;
        }
        else
        {
            _indexer.Resume();
            IsIndexing = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenDocumentDetail))]
    private async Task OpenDocumentDetailAsync()
    {
        if (SelectedDocument == null)
            return;

        var info = new FileInfo(SelectedDocument.Path);
        var detail = $"Název: {info.Name}\nTyp: {info.Extension}\nVelikost: {info.Length} B\nZměněno: {info.LastWriteTime}";
        await _dialogs.ShowInformation(detail, "Detail souboru");
    }

    private bool CanOpenDocumentDetail() => SelectedDocument != null;

    [RelayCommand]
    private void OpenSettings() => _navigation.Navigate(typeof(SettingsPage));

    [RelayCommand]
    private void Exit() => System.Windows.Application.Current.Shutdown();

    partial void OnFromDateChanged(DateTime? value) => RestartQuery();

    partial void OnToDateChanged(DateTime? value) => RestartQuery();

    partial void OnAuthorFilterChanged(string value) => RestartQuery();

    partial void OnVersionFilterChanged(string value) => RestartQuery();

    partial void OnSortFieldChanged(string value) => UpdateSort();
    partial void OnSortAscendingChanged(bool value) => UpdateSort();

    partial void OnSelectedDocumentChanged(SearchHit? value)
    {
        OpenDocumentCommand.NotifyCanExecuteChanged();
        OpenDocumentDetailCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsIndexingChanged(bool value)
    {
        OnPropertyChanged(nameof(PauseResumeText));
        OnPropertyChanged(nameof(PauseResumeSymbol));
    }

    private void UpdateSort()
    {
        var dir = SortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
        var view = ResultsView.View;
        if (view == null) return;
        view.SortDescriptions.Clear();
        var field = SortField == nameof(SearchHit.FileName) ? nameof(SearchHit.SortKey) : SortField;
        view.SortDescriptions.Add(new SortDescription(field, dir));
        if (SortField != nameof(SearchHit.FileName))
            view.SortDescriptions.Add(new SortDescription(nameof(SearchHit.SortKey), ListSortDirection.Ascending));
    }
}
