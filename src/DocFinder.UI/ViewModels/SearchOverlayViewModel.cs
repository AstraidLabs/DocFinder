using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    [ObservableProperty]
    private SearchHit? _selectedResult;

    public SearchOverlayViewModel(ISearchService searchService)
    {
        _searchService = searchService;
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
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (SelectedResult != null)
        {
            var psi = new System.Diagnostics.ProcessStartInfo(SelectedResult.Path) { UseShellExecute = true };
            System.Diagnostics.Process.Start(psi);
        }
    }
}
