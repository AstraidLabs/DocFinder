using DocFinder.Domain;
using FileEntity = DocFinder.Domain.File;

namespace DocFinder.Services;

public class FileService : RepositoryService<FileEntity>
{
    public FileService(IFileRepository repository) : base(repository) { }
}
