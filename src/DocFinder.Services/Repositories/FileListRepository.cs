using DocFinder.Domain;

namespace DocFinder.Services;

public class FileListRepository : EfRepository<FileList>, IFileListRepository
{
    public FileListRepository(DocumentDbContext context) : base(context) { }
}
