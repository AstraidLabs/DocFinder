using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DocFinder.Domain;

namespace DocFinder.App.Services;

public sealed class DocumentViewService : IDocumentViewService
{
    public UIElement GetViewer(SearchHit hit)
    {
        var ext = Path.GetExtension(hit.Path).ToLowerInvariant();
        if (ext == ".pdf")
        {
            var browser = new WebBrowser();
            browser.Navigate(new Uri(hit.Path));
            return browser;
        }
        if (ext == ".doc" || ext == ".docx")
        {
            return new System.Windows.Controls.TextBlock { Text = "Náhled DOC/DOCX není podporován. Použijte tlačítko Otevřít." };
        }
        return new System.Windows.Controls.TextBlock { Text = "Náhled není k dispozici." };
    }
}
