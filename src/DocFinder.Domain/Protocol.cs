using System;

namespace DocFinder.Domain;

public sealed class Protocol
{
    // For EF
    private Protocol() { }

    public Protocol(
        Guid id,
        File file,
        string title,
        string referenceNumber,
        ProtocolType type,
        DateTime issueDateUtc,
        string issuedBy,
        string? legalBasis,
        string responsiblePerson,
        bool print = false,
        bool electronicVersion = false,
        bool contract = false)
    {
        Id = id;
        SetFile(file);

        SetTitle(title);
        SetReferenceNumber(referenceNumber);
        ChangeType(type);

        SetIssueDate(issueDateUtc);
        SetIssuedBy(issuedBy);
        SetLegalBasis(legalBasis);
        SetResponsiblePerson(responsiblePerson);

        SetPrint(print);
        SetElectronicVersion(electronicVersion);
        SetContract(contract);

        Status = ProtocolStatus.Draft;
        CreatedUtc = DateTime.UtcNow;
        ModifiedUtc = CreatedUtc;
    }

    // --- Keys & relations ---
    public Guid Id { get; private set; }
    public Guid FileId { get; private set; }
    public File File { get; private set; } = null!;

    public Guid? AttachmentsListId { get; private set; }
    public FileList? AttachmentsList { get; private set; }

    // --- List-facing properties (metadata) ---
    public string Title { get; private set; } = string.Empty;
    public string ReferenceNumber { get; private set; } = string.Empty;
    public ProtocolType Type { get; private set; }

    public string OrganizationalUnit { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public Guid? AssetId { get; private set; }

    public string IssuedBy { get; private set; } = string.Empty;
    public string? ApprovedBy { get; private set; }
    public string? LegalBasis { get; private set; }
    public string ResponsiblePerson { get; private set; } = string.Empty;

    public DateTime IssueDateUtc { get; private set; }
    public DateTime? EffectiveFromUtc { get; private set; }
    public DateTime? ExpiresOnUtc { get; private set; }

    public ProtocolStatus Status { get; private set; }
    public string? Version { get; private set; }
    public string? Notes { get; private set; }

    // --- New flags ---
    public bool Print { get; private set; }
    public bool ElectronicVersion { get; private set; }
    public bool Contract { get; private set; }

    // --- Audit ---
    public DateTime CreatedUtc { get; private set; }
    public DateTime ModifiedUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // ----------------- Behaviour -----------------

    public void SetFile(File file)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        FileId = file.FileId;
        Touch();
    }

    public void SetTitle(string title) => Set(ref title, v => Title = v);
    public void SetReferenceNumber(string reference) => Set(ref reference, v => ReferenceNumber = v);
    public void ChangeType(ProtocolType type) { Type = type; Touch(); }

    public void SetIssuedBy(string issuedBy) => Set(ref issuedBy, v => IssuedBy = v);
    public void SetLegalBasis(string? text) { LegalBasis = text?.Trim(); Touch(); }
    public void SetResponsiblePerson(string person) => Set(ref person, v => ResponsiblePerson = v);

    public void SetOrganizationalUnit(string? unit) { OrganizationalUnit = unit?.Trim() ?? string.Empty; Touch(); }
    public void SetLocation(string? location)       { Location = location?.Trim() ?? string.Empty; Touch(); }
    public void SetAsset(Guid? assetId)             { AssetId = assetId; Touch(); }

    public void SetVersion(string? version) { Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim(); Touch(); }
    public void SetNotes(string? note)      { Notes = string.IsNullOrWhiteSpace(note) ? null : note.Trim(); Touch(); }

    // New flags setters
    public void SetPrint(bool value) { Print = value; Touch(); }
    public void SetElectronicVersion(bool value) { ElectronicVersion = value; Touch(); }
    public void SetContract(bool value) { Contract = value; Touch(); }

    public void SetIssueDate(DateTime utc)
    {
        EnsureUtc(utc, nameof(utc));
        IssueDateUtc = utc;
        if (EffectiveFromUtc.HasValue && EffectiveFromUtc.Value < IssueDateUtc)
            throw new InvalidOperationException("Effective date cannot precede issue date.");
        Touch();
    }

    public void Approve(string approvedBy, DateTime effectiveFromUtc)
    {
        EnsureUtc(effectiveFromUtc, nameof(effectiveFromUtc));
        if (effectiveFromUtc < IssueDateUtc)
            throw new InvalidOperationException("Effective date cannot precede issue date.");

        ApprovedBy = Require(approvedBy);
        EffectiveFromUtc = effectiveFromUtc;
        Status = ProtocolStatus.Approved;

        if (ExpiresOnUtc.HasValue && ExpiresOnUtc.Value < EffectiveFromUtc.Value)
            throw new InvalidOperationException("Expiration cannot precede the effective date.");
        Touch();
    }

    public void SetExpiration(DateTime? expiresOnUtc)
    {
        if (expiresOnUtc.HasValue)
        {
            EnsureUtc(expiresOnUtc.Value, nameof(expiresOnUtc));
            if (EffectiveFromUtc.HasValue && expiresOnUtc.Value < EffectiveFromUtc.Value)
                throw new InvalidOperationException("Expiration cannot precede the effective date.");
        }
        ExpiresOnUtc = expiresOnUtc;
        Touch();
    }

    public void Archive() { Status = ProtocolStatus.Archived; Touch(); }

    public void MarkReplaced(string? note = null)
    {
        Status = ProtocolStatus.Replaced;
        if (!string.IsNullOrWhiteSpace(note)) SetNotes(note);
        Touch();
    }

    public void AttachAttachmentsList(FileList list)
    {
        AttachmentsList = list ?? throw new ArgumentNullException(nameof(list));
        AttachmentsListId = list.Id;
        Touch();
    }

    // ----------------- Helpers -----------------

    private void Touch(DateTime? utc = null)
    {
        var now = utc ?? DateTime.UtcNow;
        EnsureUtc(now, nameof(utc));
        if (now < CreatedUtc) now = CreatedUtc;
        ModifiedUtc = now;
    }

    private static void Set(ref string value, Action<string> apply)
    {
        var v = Require(value);
        apply(v);
    }

    private static string Require(string v) =>
        string.IsNullOrWhiteSpace(v) ? throw new ArgumentException("Value is required.") : v.Trim();

    private static void EnsureUtc(DateTime dt, string name)
    {
        if (dt.Kind != DateTimeKind.Utc) throw new ArgumentException("DateTime must be UTC.", name);
    }
}

public enum ProtocolStatus { Draft = 0, Approved = 1, Replaced = 2, Archived = 3 }
public enum ProtocolType { Waterworks = 0, Sewer = 1, WWTP = 2, Other = 9 }

