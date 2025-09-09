namespace DocFinder.App.ViewModels.Pages;

/// <summary>
/// Simple dashboard view model with a counter.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    private int _counter;

    /// <summary>Increments the counter.</summary>
    [RelayCommand]
    private void CounterIncrement() => Counter++;
}

