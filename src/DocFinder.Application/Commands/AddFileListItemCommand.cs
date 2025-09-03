using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddFileListItemCommand(FileListItem Entity) : AddEntityCommand<FileListItem>(Entity);
