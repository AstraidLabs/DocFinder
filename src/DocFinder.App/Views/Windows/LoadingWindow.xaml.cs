using Wpf.Ui.Controls;

namespace DocFinder.App.Views.Windows;

public partial class LoadingWindow : FluentWindow
{
    public LoadingWindow()
    {
        InitializeComponent();
    }

    public void SetStatus(string message)
    {
        Dispatcher.Invoke(() => StatusText.Text = message);
    }

    public void SetProgress(double value)
    {
        Dispatcher.Invoke(() => Progress.Value = value);
    }
}
