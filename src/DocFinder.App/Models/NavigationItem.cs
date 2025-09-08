using System;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace DocFinder.App.Models
{
    public class NavigationItem
    {
        public string Title { get; set; } = string.Empty;

        public SymbolRegular Symbol { get; set; }

        public Type TargetPageType { get; set; } = typeof(Page);
    }
}
