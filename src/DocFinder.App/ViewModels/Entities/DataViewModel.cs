using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Entities;

public partial class DataViewModel : ObservableObject
{
    public DataViewModel(Data data)
    {
        FileId = data.FileId;
        DataVersion = data.DataVersion;
        FileType = data.FileType;
        DataBytes = data.DataBytes;
    }

    [ObservableProperty] private Guid _fileId;
    [ObservableProperty] private string _dataVersion = string.Empty;
    [ObservableProperty] private string _fileType = string.Empty;
    [ObservableProperty] private byte[] _dataBytes = Array.Empty<byte>();
}

