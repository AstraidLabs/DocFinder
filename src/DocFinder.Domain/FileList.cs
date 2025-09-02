using System;
using System.Collections.Generic;
using System.Linq;

namespace DocFinder.Domain;

/// <summary>
/// Aggregate root representing an ordered list of files.
/// </summary>
public sealed class FileList
{
    private readonly List<FileListItem> _items = new();

    // For EF Core
    private FileList() { }

    public FileList(Guid id, string name, DateTime createdUtc, string owner)
    {
        Id = id;
        Name = Require(name);
        Owner = Require(owner);
        CreatedUtc = createdUtc;
        ModifiedUtc = createdUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public DateTime CreatedUtc { get; private set; }
    public DateTime ModifiedUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyList<FileListItem> Items => _items.OrderBy(i => i.Order).ToList();
    public int Count => _items.Count;

    public bool ContainsFile(Guid fileId) => _items.Any(i => i.FileId == fileId);

    /// <summary>
    /// Adds multiple files to the end of the list. Skips duplicates within this list.
    /// Optionally pins each item to the file's current checksum.
    /// Returns the created items in the final order.
    /// </summary>
    public IReadOnlyList<FileListItem> AddFiles(IEnumerable<File> files, bool pinToChecksum = false)
    {
        if (files is null) throw new ArgumentNullException(nameof(files));

        var created = new List<FileListItem>();
        foreach (var file in files)
        {
            if (file is null) continue;
            if (ContainsFile(file.FileId)) continue;

            var pinned = pinToChecksum ? file.Sha256 : null;
            var item = new FileListItem(
                id: Guid.NewGuid(),
                listId: Id,
                fileId: file.FileId,
                order: _items.Count,
                label: null,
                note: null,
                pinnedSha256: pinned,
                addedUtc: DateTime.UtcNow
            );
            _items.Add(item);
            created.Add(item);
        }

        if (created.Count > 0)
        {
            NormalizeOrder();
            Touch();
        }
        return created;
    }

    /// <summary>
    /// Inserts multiple files starting at a given index (keeps input order). Skips duplicates within this list.
    /// </summary>
    public IReadOnlyList<FileListItem> InsertFiles(int index, IEnumerable<File> files, bool pinToChecksum = false)
    {
        if (files is null) throw new ArgumentNullException(nameof(files));
        index = ClampIndex(index);

        var created = new List<FileListItem>();
        foreach (var file in files)
        {
            if (file is null) continue;
            if (ContainsFile(file.FileId)) continue;

            var pinned = pinToChecksum ? file.Sha256 : null;
            var item = new FileListItem(
                id: Guid.NewGuid(),
                listId: Id,
                fileId: file.FileId,
                order: index,
                label: null,
                note: null,
                pinnedSha256: pinned,
                addedUtc: DateTime.UtcNow
            );
            _items.Insert(index++, item);
            created.Add(item);
        }

        if (created.Count > 0)
        {
            NormalizeOrder();
            Touch();
        }
        return created;
    }

    /// <summary>Moves the specified file to a new position.</summary>
    public void Reorder(Guid fileId, int newIndex)
    {
        var item = _items.First(i => i.FileId == fileId);
        _items.Remove(item);
        newIndex = ClampIndex(newIndex);
        _items.Insert(newIndex, item);
        NormalizeOrder();
        Touch();
    }

    public void PinFile(Guid fileId, string sha256)
    {
        var item = _items.First(i => i.FileId == fileId);
        item.Pin(sha256);
        Touch();
    }

    public void UnpinFile(Guid fileId)
    {
        var item = _items.First(i => i.FileId == fileId);
        item.Unpin();
        Touch();
    }

    private int ClampIndex(int index) => Math.Max(0, Math.Min(index, _items.Count));

    private void NormalizeOrder()
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].SetOrder(i);
    }

    private void Touch(DateTime? modifiedUtc = null)
    {
        ModifiedUtc = modifiedUtc ?? DateTime.UtcNow;
    }

    private static string Require(string value)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required") : value.Trim();
}
