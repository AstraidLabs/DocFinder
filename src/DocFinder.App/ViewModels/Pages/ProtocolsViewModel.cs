using System;
using System.Collections.ObjectModel;
using DocFinder.Domain;
using DocFinder.UI.ViewModels.Entities;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DocFinder.ViewModels.Pages;

public partial class ProtocolsViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    public ObservableCollection<ProtocolViewModel> Protocols { get; } = new();

    public Task OnNavigatedToAsync()
    {
        if (_isInitialized)
            return Task.CompletedTask;

        // Create sample protocol with basic domain objects
        var fileData = new Data(Guid.NewGuid(), null, "text/plain", new byte[] { 0x0 });
        var file = new File(Guid.NewGuid(), "/tmp/sample.txt", "sample", "txt", DateTime.UtcNow, "system", fileData);
        var protocol = new Protocol(Guid.NewGuid(), file, "Sample protocol", "REF-001", ProtocolType.Other, DateTime.UtcNow, "Office", null, "John Doe");

        Protocols.Add(new ProtocolViewModel(protocol));

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}

