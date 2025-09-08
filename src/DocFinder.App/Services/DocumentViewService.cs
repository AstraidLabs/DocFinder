using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DocFinder.Domain;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace DocFinder.App.Services;

public sealed class DocumentViewService : IDocumentViewService
{
    public UIElement GetViewer(SearchHit hit)
    {
        var ext = Path.GetExtension(hit.Path).ToLowerInvariant();
        if (ext == ".pdf")
        {
            try
            {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                return new TextBlock { Text = "Pro zobrazení PDF je vyžadováno nainstalované WebView2 runtime." };
            }

            var webView = new WebView2();
            InitializeWebViewAsync(webView, hit.Path);
            return webView;
        }
        if (ext == ".doc" || ext == ".docx")
        {
            return new System.Windows.Controls.TextBlock { Text = "Náhled DOC/DOCX není podporován. Použijte tlačítko Otevřít." };
        }
        return new System.Windows.Controls.TextBlock { Text = "Náhled není k dispozici." };
    }

    private static async void InitializeWebViewAsync(WebView2 webView, string path)
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.Source = new Uri(path);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            // If the runtime is uninstalled after the check, show an error.
            if (webView.Parent is Panel panel)
            {
                panel.Children.Clear();
                panel.Children.Add(new TextBlock { Text = "WebView2 runtime není k dispozici." });
            }
        }
    }
}
