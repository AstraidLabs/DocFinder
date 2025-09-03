using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.UI.ViewModels.Entities;

public partial class ProtocolViewModel : ObservableObject
{
    public ProtocolViewModel(Protocol protocol)
    {
        Id = protocol.Id;
        FileId = protocol.FileId;
        AttachmentsListId = protocol.AttachmentsListId;
        AttachmentsList = protocol.AttachmentsList != null
            ? new FileListViewModel(protocol.AttachmentsList)
            : null;
        Title = protocol.Title;
        ReferenceNumber = protocol.ReferenceNumber;
        Type = protocol.Type;
        OrganizationalUnit = protocol.OrganizationalUnit;
        Location = protocol.Location;
        AssetId = protocol.AssetId;
        IssuedBy = protocol.IssuedBy;
        ApprovedBy = protocol.ApprovedBy;
        LegalBasis = protocol.LegalBasis;
        ResponsiblePerson = protocol.ResponsiblePerson;
        IssueDateUtc = protocol.IssueDateUtc;
        EffectiveFromUtc = protocol.EffectiveFromUtc;
        ExpiresOnUtc = protocol.ExpiresOnUtc;
        Status = protocol.Status;
        Version = protocol.Version;
        Notes = protocol.Notes;
        Print = protocol.Print;
        ElectronicVersion = protocol.ElectronicVersion;
        Contract = protocol.Contract;
        CreatedUtc = protocol.CreatedUtc;
        ModifiedUtc = protocol.ModifiedUtc;
        FilePath = protocol.File.FilePath;
    }

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private Guid _fileId;
    [ObservableProperty] private Guid? _attachmentsListId;
    public FileListViewModel? AttachmentsList { get; }
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _referenceNumber = string.Empty;
    [ObservableProperty] private ProtocolType _type;
    [ObservableProperty] private string _organizationalUnit = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private Guid? _assetId;
    [ObservableProperty] private string _issuedBy = string.Empty;
    [ObservableProperty] private string? _approvedBy;
    [ObservableProperty] private string? _legalBasis;
    [ObservableProperty] private string _responsiblePerson = string.Empty;
    [ObservableProperty] private DateTime _issueDateUtc;
    [ObservableProperty] private DateTime? _effectiveFromUtc;
    [ObservableProperty] private DateTime? _expiresOnUtc;
    [ObservableProperty] private ProtocolStatus _status;
    [ObservableProperty] private string? _version;
    [ObservableProperty] private string? _notes;
    [ObservableProperty] private bool _print;
    [ObservableProperty] private bool _electronicVersion;
    [ObservableProperty] private bool _contract;
    [ObservableProperty] private DateTime _createdUtc;
    [ObservableProperty] private DateTime _modifiedUtc;

    public string FilePath { get; }
}

