using System;
using System.Windows.Forms;

namespace DocFinder.Services;

public interface ITrayService : IDisposable
{
    void Initialize(Action showOverlay, Action exitApp, Action showSettings);
}

public sealed class TrayService : ITrayService
{
    private NotifyIcon? _icon;

    public void Initialize(Action showOverlay, Action exitApp, Action showSettings)
    {
        _icon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "DocFinder"
        };
        var menu = new ContextMenuStrip();
        menu.Items.Add("Search", null, (_, _) => showOverlay());
        menu.Items.Add("Settings", null, (_, _) => showSettings());
        menu.Items.Add("Exit", null, (_, _) => exitApp());
        _icon.ContextMenuStrip = menu;
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                showOverlay();
        };
    }

    public void Dispose()
    {
        if (_icon != null)
        {
            _icon.Visible = false;
            _icon.Dispose();
            _icon = null;
        }
    }
}
