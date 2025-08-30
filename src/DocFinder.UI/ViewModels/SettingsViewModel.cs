using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocFinder.Domain.Settings;
using DocFinder.Services;
using System.Threading.Tasks;

namespace DocFinder.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private AppSettings _settings = new();

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = settingsService.Current;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _settingsService.SaveAsync(Settings);
    }
}
