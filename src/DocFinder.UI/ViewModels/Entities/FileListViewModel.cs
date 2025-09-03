using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.UI.ViewModels.Entities;

public partial class FileListViewModel : ObservableObject
{
    public FileListViewModel(FileList list)
    {
        Id = list.Id;
        Name = list.Name;
        Owner = list.Owner;
        CreatedUtc = list.CreatedUtc;
        ModifiedUtc = list.ModifiedUtc;
        Items = new ObservableCollection<FileListItemViewModel>(list.Items.Select(i => new FileListItemViewModel(i)));
    }

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _owner = string.Empty;
    [ObservableProperty] private DateTime _createdUtc;
    [ObservableProperty] private DateTime _modifiedUtc;

    public ObservableCollection<FileListItemViewModel> Items { get; }
}

