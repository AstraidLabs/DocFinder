using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DocFinder.Domain;

namespace DocFinder.UI.Services;

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
            return new TextBlock { Text = "Náhled DOC/DOCX není podporován. Použijte tlačítko Otevřít." };
        }
        return new TextBlock { Text = "Náhled není k dispozici." };
    }
}
