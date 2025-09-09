using System.Diagnostics;

namespace DocFinder.App.Services;

/// <summary>
/// Provides functionality to open documents using the default application.
/// </summary>
public sealed class DocumentOpener : IDocumentOpener
{
    /// <inheritdoc />
    public void Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var info = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };

        try
        {
            Process.Start(info);
        }
        catch
        {
            // Ignore failures to open the document
        }
    }
}

