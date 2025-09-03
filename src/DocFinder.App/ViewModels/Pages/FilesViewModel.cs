using System;
using System.Collections.ObjectModel;
using DocFinder.Domain;
using DocFinder.UI.ViewModels.Entities;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DocFinder.ViewModels.Pages;

public partial class FilesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;

    public ObservableCollection<FileViewModel> Files { get; } = new();

    public Task OnNavigatedToAsync()
    {
        if (_isInitialized)
            return Task.CompletedTask;

        var data = new Data(Guid.NewGuid(), null, "text/plain", new byte[] { 0x0 });
        var file = new File(Guid.NewGuid(), "/tmp/file.txt", "File.txt", "txt", DateTime.UtcNow, "user", data);
        Files.Add(new FileViewModel(file));

        _isInitialized = true;
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;
}

