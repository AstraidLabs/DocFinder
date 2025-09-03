using DocFinder.Domain;

namespace DocFinder.Services;

public class FileListItemService : RepositoryService<FileListItem>
{
    public FileListItemService(IFileListItemRepository repository) : base(repository) { }
}
