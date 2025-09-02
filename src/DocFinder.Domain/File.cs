using System;
using System.Security.Cryptography;

namespace DocFinder.Domain;

/// <summary>
/// Rich entity representing a file stored in the catalog.
/// Handles validation, normalization and exposes safe mutations.
/// </summary>
public sealed class File
{
    // For EF Core materialization
    private File() { }

    /// <summary>
    /// Creates a new <see cref="File"/> with initial metadata and content.
    /// Derived properties such as <see cref="Sha256"/> and <see cref="SizeBytes"/>
    /// are computed automatically from <paramref name="data"/>.
    /// </summary>
    public File(
        Guid fileId,
        string filePath,
        string name,
        string ext,
        DateTime createdUtc,
        string author,
        Data data)
    {
        FileId = fileId;

        Rename(name);
        SetExt(ext);
        Move(filePath);
        SetAuthor(author);
        SetCreated(createdUtc);

        SetData(data ?? throw new ArgumentNullException(nameof(data)));
        RecomputeDerivedFromData();
        // Initial modification equals creation
        ModifiedUtc = CreatedUtc;
    }

    public Guid FileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>Normalized file extension without dot (lowercase).</summary>
    public string Ext { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;

    /// <summary>Size of <see cref="Data"/> in bytes.</summary>
    public long SizeBytes { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime ModifiedUtc { get; private set; }
    public string Sha256 { get; private set; } = string.Empty;
    /// <summary>Normalized path with forward slashes.</summary>
    public string FilePath { get; private set; } = string.Empty;

    public Data Data { get; private set; } = null!;

    /// <summary>Row version used for optimistic concurrency.</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ----------- Mutations -----------

    /// <summary>Renames the file.</summary>
    public void Rename(string name) => Name = Require(name).Trim();

    /// <summary>Sets and normalizes the file extension.</summary>
    public void SetExt(string ext)
    {
        ext = Require(ext).Trim().TrimStart('.').ToLowerInvariant();
        Ext = ext;
    }

    /// <summary>Changes author metadata.</summary>
    public void SetAuthor(string author) => Author = (author ?? string.Empty).Trim();

    /// <summary>Moves the file to a new path and normalizes separators.</summary>
    public void Move(string path)
    {
        path = Require(path).Trim();
        FilePath = NormalizePath(path);
    }

    /// <summary>Sets creation time (UTC required).</summary>
    public void SetCreated(DateTime createdUtc)
    {
        EnsureUtc(createdUtc, nameof(createdUtc));
        CreatedUtc = createdUtc;
    }

    /// <summary>
    /// Replaces binary content, recomputes derived values and updates modification time.
    /// </summary>
    public void ReplaceContent(byte[] bytes, string? mime = null, string? dataVersion = null)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));

        if (Data is null)
        {
            Data = new Data(FileId, dataVersion, mime ?? string.Empty, bytes);
            Data.AttachFile(this);
        }
        else
        {
            Data.Replace(bytes, mime, dataVersion);
        }

        RecomputeDerivedFromData();
        Touch();
    }

    /// <summary>
    /// Explicitly sets modification time. Ensures UTC and that it does not precede <see cref="CreatedUtc"/>.
    /// </summary>
    public void Touch(DateTime? modifiedUtc = null)
    {
        var now = modifiedUtc ?? DateTime.UtcNow;
        EnsureUtc(now, nameof(modifiedUtc));
        if (now < CreatedUtc)
            throw new InvalidOperationException("ModifiedUtc cannot be earlier than CreatedUtc.");
        ModifiedUtc = now;
    }

    /// <summary>Sets the backing <see cref="Data"/> instance.</summary>
    public void SetData(Data data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        data.AttachFile(this);
    }

    // ----------- Derived values -----------

    private void RecomputeDerivedFromData()
    {
        SizeBytes = Data.DataBytes.Length;
        Sha256 = ComputeSha256Hex(Data.DataBytes);
    }

    private static string Require(string value)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.") : value;

    private static void EnsureUtc(DateTime dt, string paramName)
    {
        if (dt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be in UTC.", paramName);
    }

    private static string NormalizePath(string p)
    {
        p = p.Replace('\\', '/').Trim();
        return p;
    }

    private static string ComputeSha256Hex(byte[] bytes)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
    }
}
