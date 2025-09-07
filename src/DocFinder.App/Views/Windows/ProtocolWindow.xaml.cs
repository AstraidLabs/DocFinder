using System.Windows;
using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Windows;

public partial class ProtocolWindow : FluentWindow
{
    public ProtocolWindow(string? filePathFilter = null)
    {
        InitializeComponent();
        ProtocolList.FilterByPath(filePathFilter);
    }
}
