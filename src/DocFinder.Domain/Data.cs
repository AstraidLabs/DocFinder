using System;

namespace DocFinder.Domain;

public class Data
{
    public int IdData { get; private set; }
    public Guid FileId { get; private set; }
    public File File { get; private set; } = null!;
    public string? DataVersion { get; private set; }
    public string FileType { get; private set; } = string.Empty;
    public string DataBase64 { get; private set; } = string.Empty;
    public string Md5 { get; private set; } = string.Empty;

    private Data() { }

    public Data(string fileType, string dataBase64, string md5, string? dataVersion)
    {
        if (string.IsNullOrWhiteSpace(fileType)) throw new ArgumentException("File type is required", nameof(fileType));
        if (string.IsNullOrWhiteSpace(dataBase64)) throw new ArgumentException("Content is required", nameof(dataBase64));
        if (string.IsNullOrWhiteSpace(md5)) throw new ArgumentException("MD5 is required", nameof(md5));
        FileType = fileType;
        DataBase64 = dataBase64;
        Md5 = md5;
        DataVersion = dataVersion;
    }

    internal void SetFile(File file)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        FileId = file.FileId;
    }

    public void Update(string fileType, string dataBase64, string md5, string? dataVersion)
    {
        if (string.IsNullOrWhiteSpace(fileType)) throw new ArgumentException("File type is required", nameof(fileType));
        if (string.IsNullOrWhiteSpace(dataBase64)) throw new ArgumentException("Content is required", nameof(dataBase64));
        if (string.IsNullOrWhiteSpace(md5)) throw new ArgumentException("MD5 is required", nameof(md5));
        FileType = fileType;
        DataBase64 = dataBase64;
        Md5 = md5;
        DataVersion = dataVersion;
    }
}

