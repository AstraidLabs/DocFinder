using System.Collections.Generic;

namespace DocFinder.Domain.Settings;

/// <summary>
/// Represents user modifiable settings for the application.
/// Values provided here act as design time defaults.  When the
/// settings file is missing or new properties are added in future
/// versions, these defaults are used automatically.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Optional source folder which is also added to <see cref="WatchedRoots"/> when saved.</summary>
    public string? SourceRoot { get; set; }

    /// <summary>Directories that should be monitored for new documents.</summary>
    public List<string> WatchedRoots { get; set; } = new();

    /// <summary>Whether Optical Character Recognition is enabled.</summary>
    public bool EnableOcr { get; set; } = false;

    /// <summary>User selected theme.  "Light" and "Dark" are supported.</summary>
    public string Theme { get; set; } = "Light";

    /// <summary>Automatically re-index all documents on application start.</summary>
    public bool AutoIndexOnStartup { get; set; } = true;

    /// <summary>Use fuzzy search when querying the index.</summary>
    public bool UseFuzzySearch { get; set; } = false;

    /// <summary>Number of search results returned per page.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Frequency in minutes the file system is polled for changes.</summary>
    public int PollingMinutes { get; set; } = 5;

    public string? IndexPath { get; set; }
    public string? ThumbsPath { get; set; }
}
