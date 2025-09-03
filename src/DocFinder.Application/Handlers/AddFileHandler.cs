using DocFinder.Application.Commands;
using DocFinder.Services;
using FileEntity = DocFinder.Domain.File;

namespace DocFinder.Application.Handlers;

public sealed class AddFileHandler : AddEntityHandler<FileEntity>
{
    public AddFileHandler(FileService service) : base(service) { }
}
