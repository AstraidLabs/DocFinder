using System;

namespace DocFinder.Domain;

/// <summary>
/// Binary data associated with a <see cref="File"/>.
/// Holds raw bytes together with simple metadata.
/// </summary>
public sealed class Data
{
    // For EF Core
    private Data() { }

    public Data(Guid fileId, string? dataVersion, string fileType, byte[] dataBytes)
    {
        FileId = fileId;
        Replace(dataBytes, fileType, dataVersion);
    }

    /// <summary>Primary key and foreign key to <see cref="File"/>.</summary>
    public Guid FileId { get; private set; }
    public File File { get; private set; } = null!;

    /// <summary>Optional version of the content (e.g. format or semver).</summary>
    public string DataVersion { get; private set; } = string.Empty;
    /// <summary>MIME type of the content (e.g. application/pdf).</summary>
    public string FileType { get; private set; } = string.Empty;
    /// <summary>Binary representation of the file.</summary>
    public byte[] DataBytes { get; private set; } = Array.Empty<byte>();

    /// <summary>
    /// Replaces binary content and optionally updates metadata.
    /// </summary>
    public void Replace(byte[] dataBytes, string? fileType = null, string? dataVersion = null)
    {
        DataBytes = dataBytes ?? throw new ArgumentNullException(nameof(dataBytes));
        if (!string.IsNullOrWhiteSpace(fileType))
            FileType = fileType.Trim();
        if (dataVersion is not null)
            DataVersion = dataVersion.Trim();
    }

    /// <summary>Associates this instance with a <see cref="File"/>.</summary>
    public void AttachFile(File file)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        FileId = file.FileId;
    }
}
