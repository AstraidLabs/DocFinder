using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain.Settings;

namespace DocFinder.App.ViewModels.Entities;

public partial class AppSettingsViewModel : ObservableObject
{
    public AppSettingsViewModel(AppSettings settings)
    {
        SourceRoot = settings.SourceRoot;
        WatchedRoots = new ObservableCollection<string>(settings.WatchedRoots);
        EnableOcr = settings.EnableOcr;
        Theme = settings.Theme;
        AutoIndexOnStartup = settings.AutoIndexOnStartup;
        UseFuzzySearch = settings.UseFuzzySearch;
        PageSize = settings.PageSize;
        PollingMinutes = settings.PollingMinutes;
        IndexPath = settings.IndexPath;
        ThumbsPath = settings.ThumbsPath;
    }

    [ObservableProperty] private string? _sourceRoot;
    public ObservableCollection<string> WatchedRoots { get; }
    [ObservableProperty] private bool _enableOcr;
    [ObservableProperty] private string _theme = "Light";
    [ObservableProperty] private bool _autoIndexOnStartup = true;
    [ObservableProperty] private bool _useFuzzySearch;
    [ObservableProperty] private int _pageSize = 20;
    [ObservableProperty] private int _pollingMinutes = 5;
    [ObservableProperty] private string? _indexPath;
    [ObservableProperty] private string? _thumbsPath;
}

