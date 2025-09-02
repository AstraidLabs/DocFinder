using System;

namespace DocFinder.Domain;

/// <summary>
/// Item in a <see cref="FileList"/> referencing a concrete file.
/// </summary>
public sealed class FileListItem
{
    // For EF Core
    private FileListItem() { }

    public FileListItem(Guid id, Guid listId, Guid fileId, int order, string? label, string? note, string? pinnedSha256, DateTime addedUtc)
    {
        Id = id;
        ListId = listId;
        FileId = fileId;
        Order = order;
        Label = label;
        Note = note;
        PinnedSha256 = pinnedSha256;
        AddedUtc = addedUtc;
    }

    public Guid Id { get; private set; }
    public Guid ListId { get; private set; }
    public Guid FileId { get; private set; }
    public int Order { get; private set; }
    public string? Label { get; private set; }
    public string? Note { get; private set; }
    public string? PinnedSha256 { get; private set; }
    public DateTime AddedUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public File File { get; private set; } = null!;

    internal void SetOrder(int order) => Order = order;
    internal void Pin(string sha256) => PinnedSha256 = sha256;
    internal void Unpin() => PinnedSha256 = null;
}
