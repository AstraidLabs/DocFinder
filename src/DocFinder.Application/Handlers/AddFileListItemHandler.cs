using DocFinder.Application.Commands;
using DocFinder.Domain;
using DocFinder.Services;

namespace DocFinder.Application.Handlers;

public sealed class AddFileListItemHandler : AddEntityHandler<FileListItem>
{
    public AddFileListItemHandler(FileListItemService service) : base(service) { }
}
