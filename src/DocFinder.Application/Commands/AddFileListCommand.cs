using DocFinder.Domain;

namespace DocFinder.Application.Commands;

public sealed record AddFileListCommand(FileList Entity) : AddEntityCommand<FileList>(Entity);
