using System;

namespace DocFinder.Domain;

public class Data
{
    public int IdData { get; set; }
    public Guid FileId { get; set; }
    public File File { get; set; } = null!;
    public string? DataVersion { get; set; }
    public string DataBase64 { get; set; } = string.Empty;
}
