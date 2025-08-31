using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain;
using DocFinder.Search;

namespace DocFinder.UI.ViewModels;

public partial class SearchOverlayViewModel : ObservableObject
{
    private readonly ISearchService _searchService;

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

    public SearchOverlayViewModel(ISearchService searchService)
    {
        _searchService = searchService;
        ResultsView.Source = Results;
        ResultsView.Filter += ApplyFilter; // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-filter-data-in-a-view
        UpdateSort(); // https://learn.microsoft.com/dotnet/desktop/wpf/data/how-to-sort-data-in-a-view
    }

    partial void OnQueryChanged(string value) => _ = RunQueryAsync(value);

    partial void OnFileTypeFilterChanged(string value)
    {
        if (!string.IsNullOrEmpty(Query))
            _ = RunQueryAsync(Query);
    }

    private async Task RunQueryAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            ResultsView.View.Refresh();
            return;
        }

        Dictionary<string,string>? filters = null;
        if (!string.Equals(FileTypeFilter, "all", System.StringComparison.OrdinalIgnoreCase))
        {
            filters = new Dictionary<string, string> { { "type", FileTypeFilter.ToLowerInvariant() } };
        }
        var result = await _searchService.QueryAsync(new UserQuery(value, false, filters, null, null));
        Results.Clear();
        foreach (var hit in result.Hits)
            Results.Add(hit);
        ResultsView.View.Refresh();
    }

    [RelayCommand]
    private Task Search() => RunQueryAsync(Query);

    [RelayCommand]
    private void OpenDocument()
    {
        if (SelectedDocument != null)
        {
            var psi = new System.Diagnostics.ProcessStartInfo(SelectedDocument.Path) { UseShellExecute = true };
            System.Diagnostics.Process.Start(psi);
        }
    }

    [RelayCommand]
    private void FilterByExtension(string extension)
    {
        FileTypeFilter = extension;
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
