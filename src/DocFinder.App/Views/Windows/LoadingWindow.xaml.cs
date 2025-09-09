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
        Dispatcher.BeginInvoke(new Action(() => StatusText.Text = message));
    }

    public void SetProgress(double value)
    {
        Dispatcher.BeginInvoke(new Action(() => Progress.Value = value));
    }
}
