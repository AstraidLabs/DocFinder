using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.App.ViewModels.Entities;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Pages;

/// <summary>
/// View model for listing protocols.
/// </summary>
public partial class ProtocolsViewModel : ObservableObject
{
    private readonly IProtocolRepository _repository;

    public ObservableCollection<ProtocolItem> Protocols { get; } = new();

    [ObservableProperty]
    private ProtocolItem? _selectedProtocol;

    public ProtocolsViewModel(IProtocolRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Loads protocols from the repository.</summary>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        Protocols.Clear();
        var list = await _repository.ListAsync(ct: ct);
        foreach (var p in list)
            Protocols.Add(new ProtocolItem(p.Id, p.Title, p.ReferenceNumber));
    }
}

