using System;
using System.Collections.ObjectModel;
using DocFinder.Domain;
using DocFinder.App.ViewModels.Entities;
using DocFinder.App.Services;
using DocFinder.App.Views.Pages;
using Wpf.Ui.Abstractions.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DocFinder.App.ViewModels.Pages;

public partial class FilesViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized;
    private readonly INavigationService _navigationService;

    public FilesViewModel(INavigationService navigationService)
        => _navigationService = navigationService;

    public ObservableCollection<FileViewModel> Files { get; } = new();

    [ObservableProperty]
    private FileViewModel? selectedFile;

    [RelayCommand]
    private void OpenFile(FileViewModel? file)
    {
        if (file is null)
            return;

        NavigationState.SelectedFile = file;
        _navigationService.Navigate(typeof(FileDetailPage));
    }

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

