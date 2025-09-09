using System;

namespace DocFinder.App.Services;

/// <summary>
/// Provides an abstraction for opening documents using the host operating system.
/// </summary>
public interface IDocumentOpener
{
    /// <summary>
    /// Opens the specified file using the default associated application.
    /// </summary>
    /// <param name="path">Full path to the file.</param>
    void Open(string path);
}

