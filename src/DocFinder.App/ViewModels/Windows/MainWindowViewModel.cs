using DocFinder.App.Models;
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
        private ObservableCollection<NavigationItem> _menuItems = new()
        {
            new()
            {
                Title = "Home",
                Symbol = SymbolRegular.Home24,
                TargetPageType = typeof(DashboardPage)
            },
            new()
            {
                Title = "Data",
                Symbol = SymbolRegular.DataHistogram24,
                TargetPageType = typeof(DataPage)
            },
            new()
            {
                Title = "Protocols",
                Symbol = SymbolRegular.DocumentBulletList24,
                TargetPageType = typeof(ProtocolsPage)
            },
            new()
            {
                Title = "Files",
                Symbol = SymbolRegular.Document24,
                TargetPageType = typeof(FilesPage)
            },
            new()
            {
                Title = "Search",
                Symbol = SymbolRegular.Search24,
                TargetPageType = typeof(SearchPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<NavigationItem> _footerMenuItems = new()
        {
            new()
            {
                Title = "Settings",
                Symbol = SymbolRegular.Settings24,
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
