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
        ResultsView.Filter += ApplyFilter; // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-filter-data-in-a-view
        UpdateSort(); // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-sort-data-in-a-view
    }

    partial void OnQueryChanged(string value)
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _ = RunQueryAsync(value, _cts.Token);
    }

    partial void OnFileTypeFilterChanged(string value)
    {
        if (!string.IsNullOrEmpty(Query))
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _ = RunQueryAsync(Query, _cts.Token);
        }
    }

    private async Task RunQueryAsync(string value, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            ResultsView.View.Refresh();
            return;
        }

        Dictionary<string, string>? filters = null;
        if (!string.Equals(FileTypeFilter, "all", StringComparison.OrdinalIgnoreCase))
            filters = new Dictionary<string, string> { { "type", FileTypeFilter.ToLowerInvariant() } };

        var query = new UserQuery(value) { Filters = filters ?? new Dictionary<string, string>() };
        var result = await _searchService.QueryAsync(query, ct);

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Results.Clear();
            foreach (var hit in result.Hits)
                Results.Add(hit);
            ResultsView.View.Refresh();
        });
    }

    [RelayCommand]
    private Task Search()
    {
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        return RunQueryAsync(Query, _cts.Token);
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

    private void ApplyFilter(object? sender, FilterEventArgs e)
    {
        if (e.Item is not SearchHit hit)
        {
            e.Accepted = false;
            return;
        }
        if (FromDate.HasValue && hit.ModifiedUtc < FromDate.Value)
        {
            e.Accepted = false;
            return;
        }
        if (ToDate.HasValue && hit.ModifiedUtc > ToDate.Value)
        {
            e.Accepted = false;
            return;
        }
        if (!string.IsNullOrWhiteSpace(AuthorFilter) && !string.Equals(hit.Author, AuthorFilter, StringComparison.OrdinalIgnoreCase))
        {
            e.Accepted = false;
            return;
        }
        if (!string.IsNullOrWhiteSpace(VersionFilter) && !string.Equals(hit.Version, VersionFilter, StringComparison.OrdinalIgnoreCase))
        {
            e.Accepted = false;
            return;
        }
        e.Accepted = true;
    }

    partial void OnFromDateChanged(DateTime? value) => ResultsView.View.Refresh();
    partial void OnToDateChanged(DateTime? value) => ResultsView.View.Refresh();
    partial void OnAuthorFilterChanged(string value) => ResultsView.View.Refresh();
    partial void OnVersionFilterChanged(string value) => ResultsView.View.Refresh();
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
