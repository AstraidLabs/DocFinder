using System;

namespace DocFinder.Domain;

public sealed class ProtocolListItem
{
    // For EF Core
    private ProtocolListItem() { }

    internal ProtocolListItem(Guid id, Guid listId, Guid protocolId, int order,
                              string? label, string? note,
                              string? pinnedVersion, string? pinnedFileSha256,
                              DateTime addedUtc)
    {
        Id = id;
        ListId = listId;
        ProtocolId = protocolId;

        SetOrder(order);
        SetLabel(label);
        SetNote(note);

        if (!string.IsNullOrWhiteSpace(pinnedVersion))     PinnedVersion = pinnedVersion.Trim();
        if (!string.IsNullOrWhiteSpace(pinnedFileSha256))  PinnedFileSha256 = pinnedFileSha256.Trim();

        SetAdded(addedUtc);
    }

    public Guid Id { get; private set; }
    public Guid ListId { get; private set; }

    public Guid ProtocolId { get; private set; }
    public Protocol? Protocol { get; private set; } // optional navigation

    /// <summary>Zero-based order within the list.</summary>
    public int Order { get; private set; }

    public string? Label { get; private set; }
    public string? Note  { get; private set; }

    /// <summary>If set, this item is pinned to a specific protocol version string.</summary>
    public string? PinnedVersion { get; private set; }

    /// <summary>If set, this item is pinned to the protocol's file SHA-256.</summary>
    public string? PinnedFileSha256 { get; private set; }

    public DateTime AddedUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ---- item-level behaviour ----
    internal void SetOrder(int order)
    {
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order));
        Order = order;
    }

    public void SetLabel(string? label) => Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
    public void SetNote (string? note)  => Note  = string.IsNullOrWhiteSpace(note)  ? null : note.Trim();

    public void PinToVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version required.", nameof(version));
        PinnedVersion = version.Trim();
    }

    public void UnpinVersion() => PinnedVersion = null;

    public void PinToFileChecksum(string sha256)
    {
        if (string.IsNullOrWhiteSpace(sha256)) throw new ArgumentException("Checksum required.", nameof(sha256));
        PinnedFileSha256 = sha256.Trim();
    }

    public void UnpinFileChecksum() => PinnedFileSha256 = null;

    public void SetAdded(DateTime addedUtc)
    {
        if (addedUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("DateTime must be UTC.", nameof(addedUtc));
        AddedUtc = addedUtc;
    }
}
