using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DocFinder.Domain;

public sealed class ProtocolList
{
    // For EF Core
    private ProtocolList() { }

    public ProtocolList(Guid id, string name, DateTime createdUtc, string owner)
    {
        Id = id;
        Rename(name);
        SetOwner(owner);
        SetCreated(createdUtc);
        ModifiedUtc = CreatedUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Owner { get; private set; } = string.Empty;

    public bool IsArchived { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime ModifiedUtc { get; private set; }

    /// <summary>Optimistic concurrency token.</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private readonly List<ProtocolListItem> _items = new();
    public IReadOnlyList<ProtocolListItem> Items => new ReadOnlyCollection<ProtocolListItem>(_items);
    public int Count => _items.Count;

    // ---------- Behaviour ----------

    public void Rename(string name) => Name = Require(name).Trim();
    public void SetDescription(string? description) => Description = description?.Trim();
    public void SetOwner(string owner) => Owner = Require(owner).Trim();

    public void SetCreated(DateTime createdUtc)
    {
        EnsureUtc(createdUtc, nameof(createdUtc));
        CreatedUtc = createdUtc;
    }

    public void Touch(DateTime? modifiedUtc = null)
    {
        var now = modifiedUtc ?? DateTime.UtcNow;
        EnsureUtc(now, nameof(modifiedUtc));
        if (now < CreatedUtc) throw new InvalidOperationException("ModifiedUtc cannot be earlier than CreatedUtc.");
        ModifiedUtc = now;
    }

    public void Archive()   { if (!IsArchived) { IsArchived = true;  Touch(); } }
    public void Unarchive() { if (IsArchived)  { IsArchived = false; Touch(); } }

    public bool ContainsProtocol(Guid protocolId) => _items.Any(i => i.ProtocolId == protocolId);

    /// <summary>Add a protocol to the end of the list. Prevents duplicates.</summary>
    public ProtocolListItem AddProtocol(Protocol protocol, string? label = null, string? note = null,
                                        bool pinToProtocolVersion = false, bool pinToFileChecksum = false)
        => InsertProtocol(Count, protocol, label, note, pinToProtocolVersion, pinToFileChecksum);

    /// <summary>Insert a protocol at a specific index. Prevents duplicates.</summary>
    public ProtocolListItem InsertProtocol(int index, Protocol protocol, string? label = null, string? note = null,
                                           bool pinToProtocolVersion = false, bool pinToFileChecksum = false)
    {
        if (protocol is null) throw new ArgumentNullException(nameof(protocol));
        if (ContainsProtocol(protocol.Id))
            throw new InvalidOperationException($"Protocol {protocol.Id} is already in the list.");

        index = ClampIndex(index);

        var pinnedVersion  = pinToProtocolVersion ? protocol.Version : null;
        var pinnedChecksum = pinToFileChecksum   ? protocol.File?.Sha256 : null;

        var item = new ProtocolListItem(
            id: Guid.NewGuid(),
            listId: Id,
            protocolId: protocol.Id,
            order: index,
            label: label,
            note: note,
            pinnedVersion: pinnedVersion,
            pinnedFileSha256: pinnedChecksum,
            addedUtc: DateTime.UtcNow
        );

        _items.Insert(index, item);
        NormalizeOrder();
        Touch();
        return item;
    }

    /// <summary>Add multiple protocols (skips duplicates, keeps input order).</summary>
    public IReadOnlyList<ProtocolListItem> AddProtocols(IEnumerable<Protocol> protocols,
        bool pinToProtocolVersion = false, bool pinToFileChecksum = false)
    {
        if (protocols is null) throw new ArgumentNullException(nameof(protocols));

        var created = new List<ProtocolListItem>();
        foreach (var protocol in protocols)
        {
            if (protocol is null) continue;
            if (ContainsProtocol(protocol.Id)) continue;

            var item = new ProtocolListItem(
                id: Guid.NewGuid(),
                listId: Id,
                protocolId: protocol.Id,
                order: _items.Count,
                label: null,
                note: null,
                pinnedVersion: pinToProtocolVersion ? protocol.Version : null,
                pinnedFileSha256: pinToFileChecksum ? protocol.File?.Sha256 : null,
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

    public void RemoveProtocol(Guid protocolId)
    {
        var idx = _items.FindIndex(i => i.ProtocolId == protocolId);
        if (idx < 0) return;
        _items.RemoveAt(idx);
        NormalizeOrder();
        Touch();
    }

    public void Clear()
    {
        if (_items.Count == 0) return;
        _items.Clear();
        Touch();
    }

    public void Reorder(Guid protocolId, int newIndex)
    {
        var current = _items.FindIndex(i => i.ProtocolId == protocolId);
        if (current < 0) throw new InvalidOperationException($"Protocol {protocolId} not found in the list.");

        newIndex = ClampIndex(newIndex);
        if (current == newIndex) return;

        var item = _items[current];
        _items.RemoveAt(current);
        _items.Insert(newIndex, item);
        NormalizeOrder();
        Touch();
    }

    public void PinToVersion(Guid protocolId, string version)
    {
        var item = GetItem(protocolId);
        item.PinToVersion(Require(version));
        Touch();
    }

    public void UnpinVersion(Guid protocolId)
    {
        var item = GetItem(protocolId);
        item.UnpinVersion();
        Touch();
    }

    public void PinToFileChecksum(Guid protocolId, string sha256)
    {
        var item = GetItem(protocolId);
        item.PinToFileChecksum(Require(sha256));
        Touch();
    }

    public void UnpinFileChecksum(Guid protocolId)
    {
        var item = GetItem(protocolId);
        item.UnpinFileChecksum();
        Touch();
    }

    public void SetItemLabel(Guid protocolId, string? label) { GetItem(protocolId).SetLabel(label); Touch(); }
    public void SetItemNote (Guid protocolId, string? note)  { GetItem(protocolId).SetNote(note);  Touch(); }

    // ---------- helpers ----------
    private ProtocolListItem GetItem(Guid protocolId)
        => _items.FirstOrDefault(i => i.ProtocolId == protocolId)
           ?? throw new InvalidOperationException($"Protocol {protocolId} not found in the list.");

    private int ClampIndex(int index) => Math.Max(0, Math.Min(index, _items.Count));

    private void NormalizeOrder()
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].SetOrder(i);
    }

    private static string Require(string v)
        => string.IsNullOrWhiteSpace(v) ? throw new ArgumentException("Value is required.") : v;

    private static void EnsureUtc(DateTime dt, string paramName)
    {
        if (dt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime must be in UTC.", paramName);
    }
}
