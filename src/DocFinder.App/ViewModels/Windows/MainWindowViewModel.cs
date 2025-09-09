using System.Collections.ObjectModel;
using System.Linq;
using DocFinder.App.Services;
using DocFinder.App.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DocFinder.App.ViewModels.Windows;

/// <summary>
/// View model for the application's main window.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly ISnackbarService _snackbarService;
    private readonly IMessageDialogService _dialogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    public MainWindowViewModel(
        INavigationService navigationService,
        IThemeService themeService,
        ISnackbarService snackbarService,
        IMessageDialogService dialogService)
    {
        _navigationService = navigationService;
        _themeService = themeService;
        _snackbarService = snackbarService;
        _dialogService = dialogService;

        ApplicationTitle = "DocFinder";
        IsDarkTheme = _themeService.GetTheme() == ApplicationTheme.Dark;
        BuildMenu();
    }

    /// <summary>Application title displayed in the window.</summary>
    [ObservableProperty]
    private string _applicationTitle;

    /// <summary>Indicates whether dark theme is active.</summary>
    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private object? _selectedItem;

    /// <summary>Navigation menu items.</summary>
    public ObservableCollection<object> MenuItems { get; } = new();

    /// <summary>Footer navigation menu items.</summary>
    public ObservableCollection<object> FooterMenuItems { get; } = new();

    /// <summary>Footer text displayed at the bottom of the window.</summary>
    [ObservableProperty]
    private string _footerText = $"© {DateTime.Now.Year} DocFinder";

    private void BuildMenu()
    {
        MenuItems.Add(new NavigationViewItem
        {
            Content = "Home",
            Tag = "dashboard",
            Icon = new SymbolIcon(SymbolRegular.Home24),
            TargetPageType = typeof(DashboardPage)
        });
        MenuItems.Add(new NavigationViewItem
        {
            Content = "Data",
            Tag = "data",
            Icon = new SymbolIcon(SymbolRegular.DataHistogram24),
            TargetPageType = typeof(DataPage)
        });
        MenuItems.Add(new NavigationViewItem
        {
            Content = "Protocols",
            Tag = "protocols",
            Icon = new SymbolIcon(SymbolRegular.DocumentBulletList24),
            TargetPageType = typeof(ProtocolsPage)
        });
        MenuItems.Add(new NavigationViewItem
        {
            Content = "Files",
            Tag = "files",
            Icon = new SymbolIcon(SymbolRegular.Document24),
            TargetPageType = typeof(FilesPage)
        });
        MenuItems.Add(new NavigationViewItem
        {
            Content = "Search",
            Tag = "search",
            Icon = new SymbolIcon(SymbolRegular.Search24),
            TargetPageType = typeof(SearchPage)
        });

        FooterMenuItems.Add(new NavigationViewItem
        {
            Content = "Settings",
            Tag = "settings",
            Icon = new SymbolIcon(SymbolRegular.Settings24),
            TargetPageType = typeof(SettingsPage)
        });
        FooterMenuItems.Add(new NavigationViewItem
        {
            Content = "About",
            Tag = "about",
            Icon = new SymbolIcon(SymbolRegular.Info24)
        });
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _themeService.SetTheme(value ? ApplicationTheme.Dark : ApplicationTheme.Light);
        var message = value ? "Dark theme enabled" : "Light theme enabled";
        _snackbarService.Show(message);
    }

    /// <summary>Navigates to a page based on the provided tag.</summary>
    [RelayCommand]
    private void NavigateByTag(string tag)
    {
        var item = MenuItems.Concat(FooterMenuItems)
            .OfType<NavigationViewItem>()
            .FirstOrDefault(i => string.Equals(i.Tag as string, tag, StringComparison.OrdinalIgnoreCase));

        if (item?.TargetPageType is Type page)
        {
            _navigationService.Navigate(page);
        }
        else if (string.Equals(tag, "about", StringComparison.OrdinalIgnoreCase))
        {
            OpenAbout();
        }
    }

    /// <summary>Navigates to the settings page.</summary>
    [RelayCommand]
    private void OpenSettings() => _navigationService.Navigate(typeof(SettingsPage));

    /// <summary>Displays basic application information.</summary>
    [RelayCommand]
    private async Task OpenAbout()
    {
        await _dialogService.ShowInformation("DocFinder", "About");
    }
}

