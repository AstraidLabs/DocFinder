using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.UI.ViewModels.Entities;

public partial class ProtocolListItemViewModel : ObservableObject
{
    public ProtocolListItemViewModel(ProtocolListItem item)
    {
        Id = item.Id;
        ListId = item.ListId;
        ProtocolId = item.ProtocolId;
        Order = item.Order;
        Label = item.Label;
        Note = item.Note;
        PinnedVersion = item.PinnedVersion;
        PinnedFileSha256 = item.PinnedFileSha256;
        AddedUtc = item.AddedUtc;
    }

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private Guid _listId;
    [ObservableProperty] private Guid _protocolId;
    [ObservableProperty] private int _order;
    [ObservableProperty] private string? _label;
    [ObservableProperty] private string? _note;
    [ObservableProperty] private string? _pinnedVersion;
    [ObservableProperty] private string? _pinnedFileSha256;
    [ObservableProperty] private DateTime _addedUtc;
}

