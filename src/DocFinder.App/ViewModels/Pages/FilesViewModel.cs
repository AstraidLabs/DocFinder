using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DocFinder.App.Services;
using DocFinder.App.ViewModels.Entities;
using DocFinder.Domain;

namespace DocFinder.App.ViewModels.Pages;

/// <summary>
/// View model for displaying files.
/// </summary>
public partial class FilesViewModel : ObservableObject
{
    private readonly IFileRepository _repository;
    private readonly IDocumentOpener _opener;

    public ObservableCollection<FileItem> Files { get; } = new();

    [ObservableProperty]
    private FileItem? _selectedFile;

    public FilesViewModel(IFileRepository repository, IDocumentOpener opener)
    {
        _repository = repository;
        _opener = opener;
    }

    /// <summary>Loads files from the repository.</summary>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken ct = default)
    {
        Files.Clear();
        var files = await _repository.ListAsync(ct: ct);
        foreach (var f in files)
            Files.Add(new FileItem(f.FileId, f.FilePath, f.Name));
    }

    /// <summary>Opens the specified file using the document opener service.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private void OpenFile(FileItem? file)
    {
        var target = file ?? SelectedFile;
        if (target == null)
            return;

        _opener.Open(target.Path);
    }

    private bool CanOpenFile(FileItem? file) => (file ?? SelectedFile) != null;
}

