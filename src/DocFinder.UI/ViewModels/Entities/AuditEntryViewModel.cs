using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.UI.ViewModels.Entities;

public partial class AuditEntryViewModel : ObservableObject
{
    public AuditEntryViewModel(AuditEntry entry)
    {
        Id = entry.Id;
        DocumentId = entry.DocumentId;
        Action = entry.Action;
        Timestamp = entry.Timestamp;
        UserName = entry.UserName;
    }

    [ObservableProperty] private int _id;
    [ObservableProperty] private Guid _documentId;
    [ObservableProperty] private string _action = string.Empty;
    [ObservableProperty] private DateTime _timestamp;
    [ObservableProperty] private string _userName = string.Empty;
}

