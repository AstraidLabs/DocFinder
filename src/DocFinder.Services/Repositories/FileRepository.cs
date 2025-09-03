using FileEntity = DocFinder.Domain.File;
using DocFinder.Domain;

namespace DocFinder.Services;

public class FileRepository : EfRepository<FileEntity>, IFileRepository
{
    public FileRepository(DocumentDbContext context) : base(context) { }
}
