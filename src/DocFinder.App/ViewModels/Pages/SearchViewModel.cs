using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DocFinder.App.Services;
using DocFinder.App.Views.Pages;
using DocFinder.Application;
using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Indexing;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DocFinder.App.ViewModels.Pages;

/// <summary>
/// View model powering the search page.
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    private readonly CommandDispatcher _dispatcher;
    private readonly ISettingsService _settings;
    private readonly IIndexer _indexer;
    private readonly IMessageDialogService _dialogs;
    private readonly INavigationService _navigation;
    private readonly IDocumentOpener _opener;

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

    /// <summary>Text for the pause/resume indexing button.</summary>
    public string PauseResumeText => IsIndexing ? "Pause indexing" : "Resume indexing";

    /// <summary>Icon for the pause/resume indexing button.</summary>
    public SymbolRegular PauseResumeSymbol => IsIndexing ? SymbolRegular.PauseCircle24 : SymbolRegular.PlayCircle24;

    public SearchViewModel(
        CommandDispatcher dispatcher,
        ISettingsService settings,
        IIndexer indexer,
        IMessageDialogService dialogs,
        INavigationService navigation,
        IDocumentOpener opener)
    {
        _dispatcher = dispatcher;
        _settings = settings;
        _indexer = indexer;
        _dialogs = dialogs;
        _navigation = navigation;
        _opener = opener;

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

    /// <summary>Executes the search using the current query.</summary>
    [RelayCommand]
    private Task Search()
    {
        RestartQuery(Query);
        return _currentQuery ?? Task.CompletedTask;
    }

    /// <summary>Opens the selected document using the document opener service.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenDocument))]
    private void OpenDocument()
    {
        if (SelectedDocument == null)
            return;

        var path = SelectedDocument.Path;
        if (!System.IO.File.Exists(path))
            return;
        if (!_settings.Current.WatchedRoots.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            return;

        _opener.Open(path);
    }

    private bool CanOpenDocument() => SelectedDocument != null;

    /// <summary>Clears the current search query.</summary>
    [RelayCommand]
    private void NewSearch() => Query = string.Empty;

    /// <summary>Reindexes all documents with confirmation.</summary>
    [RelayCommand]
    private async Task ReindexAsync(CancellationToken ct)
    {
        if (!await _dialogs.ShowConfirmation("Reindex all documents?", "DocFinder"))
            return;

        try
        {
            await _indexer.ReindexAllAsync(ct);
            await _dialogs.ShowInformation("Reindex completed", "DocFinder");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowError($"Reindex failed: {ex.Message}", "DocFinder");
        }
    }

    /// <summary>Toggles indexing state.</summary>
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

    /// <summary>Shows basic information about the selected document.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenDocumentDetail))]
    private async Task OpenDocumentDetailAsync()
    {
        if (SelectedDocument == null)
            return;

        var info = new FileInfo(SelectedDocument.Path);
        var detail = $"Name: {info.Name}\nType: {info.Extension}\nSize: {info.Length} B\nModified: {info.LastWriteTime}";
        await _dialogs.ShowInformation(detail, "File details");
    }

    private bool CanOpenDocumentDetail() => SelectedDocument != null;

    /// <summary>Navigates to the settings page.</summary>
    [RelayCommand]
    private void OpenSettings() => _navigation.Navigate(typeof(SettingsPage));

    /// <summary>Exits the application.</summary>
    [RelayCommand]
    private void Exit() => System.Windows.Application.Current.Shutdown();

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

