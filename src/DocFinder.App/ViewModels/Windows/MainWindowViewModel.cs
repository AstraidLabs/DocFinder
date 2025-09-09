using System;
using System.Collections.ObjectModel;
using DocFinder.App.Views.Pages;
using Wpf.Ui.Controls;

namespace DocFinder.App.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - DocFinder";

        [ObservableProperty]
        private ObservableCollection<NavigationViewItem> _menuItems = new()
        {
            new()
            {
                Content = "Home",
                Icon = new SymbolIcon(SymbolRegular.Home24),
                TargetPageType = typeof(DashboardPage)
            },
            new()
            {
                Content = "Data",
                Icon = new SymbolIcon(SymbolRegular.DataHistogram24),
                TargetPageType = typeof(DataPage)
            },
            new()
            {
                Content = "Protocols",
                Icon = new SymbolIcon(SymbolRegular.DocumentBulletList24),
                TargetPageType = typeof(ProtocolsPage)
            },
            new()
            {
                Content = "Files",
                Icon = new SymbolIcon(SymbolRegular.Document24),
                TargetPageType = typeof(FilesPage)
            },
            new()
            {
                Content = "Search",
                Icon = new SymbolIcon(SymbolRegular.Search24),
                TargetPageType = typeof(SearchPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<NavigationViewItem> _footerMenuItems = new()
        {
            new()
            {
                Content = "Settings",
                Icon = new SymbolIcon(SymbolRegular.Settings24),
                TargetPageType = typeof(SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };

        [ObservableProperty]
        private string _footerText = $"© {DateTime.Now.Year} DocFinder";

        [RelayCommand]
        private void OpenFile()
        {
            // TODO: Implement file open logic
        }

        [RelayCommand]
        private void EditItem()
        {
            // TODO: Implement edit logic
        }
    }
}
