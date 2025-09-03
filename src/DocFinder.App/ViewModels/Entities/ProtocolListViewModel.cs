using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Entities;

public partial class ProtocolListViewModel : ObservableObject
{
    public ProtocolListViewModel(ProtocolList list)
    {
        Id = list.Id;
        Name = list.Name;
        Description = list.Description;
        Owner = list.Owner;
        IsArchived = list.IsArchived;
        CreatedUtc = list.CreatedUtc;
        ModifiedUtc = list.ModifiedUtc;
        Items = new ObservableCollection<ProtocolListItemViewModel>(list.Items.Select(i => new ProtocolListItemViewModel(i)));
    }

    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string _owner = string.Empty;
    [ObservableProperty] private bool _isArchived;
    [ObservableProperty] private DateTime _createdUtc;
    [ObservableProperty] private DateTime _modifiedUtc;

    public ObservableCollection<ProtocolListItemViewModel> Items { get; }
}

