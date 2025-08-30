using System.Collections.Generic;

namespace DocFinder.Domain.Settings;

public sealed class AppSettings
{
    public string? SourceRoot { get; set; }
    public List<string> WatchedRoots { get; set; } = new();
    public bool EnableOcr { get; set; } = true;
    public int PollingMinutes { get; set; } = 5;
    public string? IndexPath { get; set; }
    public string? ThumbsPath { get; set; }
}
