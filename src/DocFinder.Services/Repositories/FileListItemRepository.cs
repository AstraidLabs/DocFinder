using DocFinder.Domain;

namespace DocFinder.Services;

public class FileListItemRepository : EfRepository<FileListItem>, IFileListItemRepository
{
    public FileListItemRepository(DocumentDbContext context) : base(context) { }
}
