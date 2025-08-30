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

    public ObservableCollection<SearchHit> Results { get; } = new();

    [ObservableProperty]
    private SearchHit? _selectedResult;

    public SearchOverlayViewModel(ISearchService searchService)
    {
        _searchService = searchService;
    }

    partial void OnQueryChanged(string value)
    {
        _ = RunQueryAsync(value);
    }

    private async Task RunQueryAsync(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            return;
        }

        var result = await _searchService.QueryAsync(new UserQuery(value, false, null, null, null));
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
