using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain;
using DocFinder.Domain.Settings;
using DocFinder.Search;

namespace DocFinder.UI.ViewModels;

public partial class SearchOverlayViewModel : ObservableObject
{
    private readonly ISearchService _searchService;
    private readonly ISettingsService _settings;
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

    public SearchOverlayViewModel(ISearchService searchService, ISettingsService settings)
    {
        _searchService = searchService;
        _settings = settings;
        ResultsView.Source = Results;
        UpdateSort(); // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-sort-data-in-a-view
    }

    partial void OnQueryChanged(string value)
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _currentQuery = RunQueryAsync(value, _cts.Token);
    }

    partial void OnFileTypeFilterChanged(string value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _currentQuery = RunQueryAsync(Query, _cts.Token);
        }
    }

    private async Task RunQueryAsync(string value, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Results.Clear();
                ResultsView.View.Refresh();
                return;
            }

            var filters = new Dictionary<string, string>();
            if (!string.Equals(FileTypeFilter, "all", StringComparison.OrdinalIgnoreCase))
                filters["type"] = FileTypeFilter.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(AuthorFilter))
                filters["author"] = AuthorFilter;
            if (!string.IsNullOrWhiteSpace(VersionFilter))
                filters["version"] = VersionFilter;

            var query = new UserQuery(value)
            {
                Filters = filters,
                FromUtc = FromDate.HasValue ? new DateTimeOffset(FromDate.Value.ToUniversalTime()) : null,
                ToUtc = ToDate.HasValue ? new DateTimeOffset(ToDate.Value.ToUniversalTime()) : null
            };

            var result = await _searchService.QueryAsync(query, ct);

            await Application.Current.Dispatcher.InvokeAsync(() =>
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

    [RelayCommand]
    private void OpenDocument()
    {
        if (SelectedDocument == null)
            return;

        var path = SelectedDocument.Path;
        if (!File.Exists(path))
            return;
        if (!_settings.Current.WatchedRoots.Any(r => path.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
            return;
        var psi = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
        System.Diagnostics.Process.Start(psi);
    }

    partial void OnFromDateChanged(DateTime? value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _currentQuery = RunQueryAsync(Query, _cts.Token);
        }
    }

    partial void OnToDateChanged(DateTime? value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _currentQuery = RunQueryAsync(Query, _cts.Token);
        }
    }

    partial void OnAuthorFilterChanged(string value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _currentQuery = RunQueryAsync(Query, _cts.Token);
        }
    }

    partial void OnVersionFilterChanged(string value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _currentQuery = RunQueryAsync(Query, _cts.Token);
        }
    }
    partial void OnSortFieldChanged(string value) => UpdateSort();
    partial void OnSortAscendingChanged(bool value) => UpdateSort();

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
