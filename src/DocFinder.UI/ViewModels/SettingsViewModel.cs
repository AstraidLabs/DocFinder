using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using DocFinder.Indexing;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;
using System;

namespace DocFinder.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IWatcherService _watcherService;

    [ObservableProperty]
    private AppSettings _settings = new();

    public SettingsViewModel(ISettingsService settingsService, IWatcherService watcherService)
    {
        _settingsService = settingsService;
        _watcherService = watcherService;
        _settings = settingsService.Current;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveAsync(Settings);

        // Restart file watchers to reflect the updated roots
        _watcherService.UpdateRoots(Settings.WatchedRoots);

        // Apply the selected theme immediately
        var theme = Settings.Theme.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme);
    }
}
