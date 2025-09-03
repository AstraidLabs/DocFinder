using DocFinder.Domain;

namespace DocFinder.Services;

public class FileListService : RepositoryService<FileList>
{
    public FileListService(IFileListRepository repository) : base(repository) { }
}
