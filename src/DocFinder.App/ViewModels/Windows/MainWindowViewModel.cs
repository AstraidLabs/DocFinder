using DocFinder.App.Views.Pages;
﻿using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace DocFinder.App.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - DocFinder";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Home",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Data",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DataHistogram24 },
                TargetPageType = typeof(DataPage)
            },
            new NavigationViewItem()
            {
                Content = "Protocols",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentBulletList24 },
                TargetPageType = typeof(ProtocolsPage)
            },
            new NavigationViewItem()
            {
                Content = "Files",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Document24 },
                TargetPageType = typeof(FilesPage)
            },
            new NavigationViewItem()
            {
                Content = "Search",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Search24 },
                TargetPageType = typeof(SearchPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
