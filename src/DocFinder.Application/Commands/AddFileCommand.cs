using FileEntity = DocFinder.Domain.File;

namespace DocFinder.Application.Commands;

public sealed record AddFileCommand(FileEntity Entity) : AddEntityCommand<FileEntity>(Entity);
