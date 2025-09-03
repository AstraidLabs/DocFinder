using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Entities;

public partial class FileListItemViewModel : ObservableObject
{
    public FileListItemViewModel(FileListItem item)
    {
        Id = item.Id;
        FileId = item.FileId;
        Order = item.Order;
        Label = item.Label;
        Note = item.Note;
        PinnedSha256 = item.PinnedSha256;
        AddedUtc = item.AddedUtc;
    }

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private Guid _fileId;
    [ObservableProperty] private int _order;
    [ObservableProperty] private string? _label;
    [ObservableProperty] private string? _note;
    [ObservableProperty] private string? _pinnedSha256;
    [ObservableProperty] private DateTime _addedUtc;
}

