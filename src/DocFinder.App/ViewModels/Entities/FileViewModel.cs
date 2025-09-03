using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Entities;

public partial class FileViewModel : ObservableObject
{
    public FileViewModel(File file)
    {
        FileId = file.FileId;
        Name = file.Name;
        Ext = file.Ext;
        Author = file.Author;
        SizeBytes = file.SizeBytes;
        CreatedUtc = file.CreatedUtc;
        ModifiedUtc = file.ModifiedUtc;
        Sha256 = file.Sha256;
        FilePath = file.FilePath;
    }

    [ObservableProperty] private Guid _fileId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _ext = string.Empty;
    [ObservableProperty] private string _author = string.Empty;
    [ObservableProperty] private long _sizeBytes;
    [ObservableProperty] private DateTime _createdUtc;
    [ObservableProperty] private DateTime _modifiedUtc;
    [ObservableProperty] private string _sha256 = string.Empty;
    [ObservableProperty] private string _filePath = string.Empty;
}

