using System;

namespace DocFinder.Domain;

/// <summary>
/// Binary data associated with a <see cref="File"/>. Encapsulates validation and
/// provides methods to mutate content in a safe way.
/// </summary>
public class Data
{
    private Data() { }

    public Data(Guid fileId, string? dataVersion, string fileType, byte[] dataBytes, string md5)
    {
        FileId = fileId;
        SetDataVersion(dataVersion);
        SetFileType(fileType);
        UpdateBytes(dataBytes);
        SetMd5(md5);
    }

    public int Id { get; private set; }
    public Guid FileId { get; private set; }
    public File File { get; private set; } = null!;
    public string DataVersion { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;
    public byte[] DataBytes { get; private set; } = Array.Empty<byte>();
    public string Md5 { get; private set; } = string.Empty;

    public void SetDataVersion(string? version) => DataVersion = version ?? string.Empty;
    public void SetFileType(string fileType) => FileType = ValidateRequired(fileType, nameof(fileType));
    public void UpdateBytes(byte[] dataBytes) => DataBytes = dataBytes ?? Array.Empty<byte>();
    public void SetMd5(string md5) => Md5 = ValidateRequired(md5, nameof(md5));
    public void AttachFile(File file)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        FileId = file.FileId;
    }

    private static string ValidateRequired(string value, string param)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{param} required", param);
        return value;
    }
}
