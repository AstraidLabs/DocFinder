using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddFileListHandler : AddEntityHandler<FileList>
{
    public AddFileListHandler(FileListService service) : base(service) { }
}
