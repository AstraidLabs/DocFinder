namespace DocFinder.App.ViewModels.Pages;

public partial class SearchViewModel : ObservableObject, INavigationAware
{
    public Task OnNavigatedToAsync() => Task.CompletedTask;

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}
